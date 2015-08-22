namespace SharpKindleHighlightsExtractorLib

open canopy
open runner
open canopy.core

module private HelperFunctions = 
    open System

    type Highlight = string
    type Book = string
    type BookWithHighlights = Book * list<Highlight>

    let fetchHighlightsOnPage () : BookWithHighlights =
        let titles = canopy.core.elements ".title" |> List.map (fun f -> f.Text)
        let stats = canopy.core.elements ".yourHighlightsStats" |> List.map (fun f -> f.Text.Split(' ').[2] |> Int32.Parse)
        let numOfHighlights = List.head stats
        let highlights = canopy.core.elements ".highlight" |> List.map (fun f -> f.Text)
        (List.head titles), (highlights |> Seq.take numOfHighlights |> Seq.toList)    
    
    let rec crawl ()=
        let nextPage = someElement "#nextBookLink"
        [
            match nextPage with
            | Some page -> 
                yield fetchHighlightsOnPage ()
                page.GetAttribute("href") |> url
                yield! crawl ()
            | None -> ()
        ]
    
    let login user password =
        url "https://kindle.amazon.com/login"
        "#ap_email" << user
        "#ap_password" << password

        let loginSuccess () = elements ".greeting" |> Seq.length = 1

        click "#signInSubmit"

        waitFor loginSuccess
    
    
    let fetchHighlights () =
        url "https://kindle.amazon.com/your_highlights"
        crawl ()


exception UserAlreadyLoggedIn
exception UserNotLoggedIn

/// Interface for C#.
type BookWithHighlights (bookName : string, highlights : seq<string>) =
    let bookName = bookName
    let highlights = highlights

    member this.BookName with get() = bookName
    member this.Highlights with get() = highlights

/// Spawns a chrome instance for crawling kindle highlights.
type HighlightsExtractor() = 
    let mutable loggedIn = false
    do canopy.configuration.chromeDir <- __SOURCE_DIRECTORY__
    do start chrome

    /// Log in to kindle website.
    member this.LogIn (userName : string) (password : string) =
        if loggedIn then raise UserAlreadyLoggedIn            
        loggedIn <- true
        HelperFunctions.login userName password

    /// Returns IEnumerable containing bookname * IEnumrable highlights
    member this.Crawl () =
        if not loggedIn then raise UserNotLoggedIn
        HelperFunctions.fetchHighlights ()
        |> Seq.map (fun (book, highlights) -> new BookWithHighlights(book, highlights))