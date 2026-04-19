open System
open System.Timers
open Domain

// Display

let mutable bannerPos = 0

let clearAndDraw (state: State) =
    Console.Clear()
    let isActive = state.ActiveIndex.IsSome
    printfn "╔%s╗" (String('═', Banner.width))
    printfn "%s" (Banner.render bannerPos isActive)
    printfn "╚%s╝" (String('═', Banner.width))
    printfn ""

    if state.Tasks.IsEmpty then
        printfn "  (no tasks yet)"
    else
        state.Tasks |> List.iteri (fun i t ->
            let marker =
                match state.ActiveIndex with
                | Some idx when idx = i -> "▶"
                | _ -> " "
            let time = formatTime t.Seconds
            printfn " %s [%d] %-33s %s" marker (i + 1) t.Name time
        )

    printfn ""
    let total = state.Tasks |> List.sumBy (fun t -> t.Seconds)
    printfn "  Total: %s" (formatTime total)
    printfn ""
    printfn "  Commands:"
    printfn "  [a] Add task   [d <n>] Delete  [s <n>] Start/Stop"
    printfn "  [r <n>] Rename [x] Reset all   [q] Quit"
    printf  "  > "

// Main loop

[<EntryPoint>]
let main _ =
    let mutable state = { Tasks = loadTasks (); ActiveIndex = None }
    let mutable running = true

    let timer = new Timer(1000.0)
    let mutable needsRedraw = false

    timer.Elapsed.Add(fun _ ->
        match state.ActiveIndex with
        | Some idx ->
            let tasks =
                state.Tasks |> List.mapi (fun i t ->
                    if i = idx then { t with Seconds = t.Seconds + 1 } else t)
            state <- { state with Tasks = tasks }
            bannerPos <- bannerPos + 1
            needsRedraw <- true
        | None -> ()
    )
    timer.Start()

    clearAndDraw state

    while running do
        if Console.KeyAvailable || not running then
            let input = Console.ReadLine()
            match input with
            | null | "" -> ()
            | s ->
                let parts = s.Trim().Split(' ', 2)
                let cmd = parts[0].ToLower()

                match cmd with
                | "a" ->
                    let name =
                        if parts.Length > 1 then parts[1]
                        else sprintf "Task %d" (state.Tasks.Length + 1)
                    state <- { state with Tasks = state.Tasks @ [{ Name = name; Seconds = 0 }] }

                | "d" when parts.Length > 1 ->
                    match Int32.TryParse(parts[1]) with
                    | true, n when n >= 1 && n <= state.Tasks.Length ->
                        let idx = n - 1
                        let newActive =
                            match state.ActiveIndex with
                            | Some ai when ai = idx -> None
                            | Some ai when ai > idx -> Some (ai - 1)
                            | other -> other
                        state <- {
                            Tasks = state.Tasks |> List.removeAt idx
                            ActiveIndex = newActive
                        }
                    | _ -> ()

                | "s" when parts.Length > 1 ->
                    match Int32.TryParse(parts[1]) with
                    | true, n when n >= 1 && n <= state.Tasks.Length ->
                        let idx = n - 1
                        state <-
                            match state.ActiveIndex with
                            | Some ai when ai = idx -> { state with ActiveIndex = None }
                            | _ -> { state with ActiveIndex = Some idx }
                    | _ -> ()

                | "r" when parts.Length > 1 ->
                    let rParts = parts[1].Split(' ', 2)
                    match Int32.TryParse(rParts[0]) with
                    | true, n when n >= 1 && n <= state.Tasks.Length && rParts.Length > 1 ->
                        let idx = n - 1
                        let tasks =
                            state.Tasks |> List.mapi (fun i t ->
                                if i = idx then { t with Name = rParts[1] } else t)
                        state <- { state with Tasks = tasks }
                    | _ -> ()

                | "x" ->
                    let tasks = state.Tasks |> List.map (fun t -> { t with Seconds = 0 })
                    state <- { Tasks = tasks; ActiveIndex = None }

                | "q" ->
                    running <- false
                    saveTasks state.Tasks

                | _ -> ()

                if running then
                    saveTasks state.Tasks
                    clearAndDraw state
        else
            if needsRedraw then
                needsRedraw <- false
                clearAndDraw state
            Threading.Thread.Sleep(100)

    timer.Stop()
    printfn "\n  Saved. Bye!"
    0
