module Banner
open System
let width = 50
let private trail = [| '░'; '░'; '▒'; '▓' |]
let private tLen = trail.Length
let private fullCycle = (width + tLen - 1) * 2
let private halfCycle = width + tLen - 1
let render (pos: int) (isActive: bool) =
    if not isActive then
        let text = "TASK TIME TRACKER"
        let pad = (width - text.Length) / 2
        sprintf "║%s%s%s║" (String(' ', pad)) text (String(' ', width - text.Length - pad))
    else
        let p = pos % fullCycle
        let goingRight = p < halfCycle
        let phase = if goingRight then p else p - halfCycle
        let line = Array.create width ' '
        let headPos =
            if goingRight then min phase (width - 1)
            else width - 1 - min phase (width - 1)
        let squish = max 0 (phase - (width - 1))
        let vis = tLen - squish
        if goingRight then
            for j in 0 .. vis - 1 do
                let pos = headPos - (vis - 1) + j
                if pos >= 0 && pos < width then
                    line[pos] <- trail[tLen - vis + j]
        else
            for j in 0 .. vis - 1 do
                let pos = headPos + j
                if pos >= 0 && pos < width then
                    line[pos] <- trail[tLen - 1 - j]
        sprintf "║%s║" (String(line))
