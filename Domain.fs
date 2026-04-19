module Domain

open System
open System.IO
open System.Text.Json
open System.Text.Json.Serialization.Metadata

// Types

type Task = { Name: string; Seconds: int }

type State = {
    Tasks: Task list
    ActiveIndex: int option
}

let formatTime s =
    sprintf "%02d:%02d:%02d" (s / 3600) (s % 3600 / 60) (s % 60)

// Persistence

let private jsonOptions =
    let opts = JsonSerializerOptions(WriteIndented = true)
    opts.TypeInfoResolver <- DefaultJsonTypeInfoResolver()
    opts

let savePath =
    let dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TaskTimeTracker")
    Directory.CreateDirectory(dir) |> ignore
    Path.Combine(dir, "tasks-console.json")

let saveTasks (tasks: Task list) =
    let json = JsonSerializer.Serialize(tasks, jsonOptions)
    File.WriteAllText(savePath, json)

let loadTasks () =
    if File.Exists(savePath) then
        try JsonSerializer.Deserialize<Task list>(File.ReadAllText(savePath), jsonOptions)
        with _ -> []
    else []
