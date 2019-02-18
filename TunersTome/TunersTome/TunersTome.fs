// Copyright 2018 Fabulous contributors. See LICENSE.md for license.
namespace TunersTome

open System.Diagnostics
open Fabulous.Core
open Fabulous.DynamicViews
open Xamarin.Forms

module App = 
    open Xamarin.Forms

    type Player =
        { ID : int
          Life : int
          Poison : int
          Deck : string }

    type Model = 
       { Players: Player list }
    
    type Msg = 
        | UpdateLife of int * int (* pid, point *)
        | Reset

    
    let defaultPlayer = { Life = 20; Poison = 0; Deck = ""; ID = -1}
    
    let initModel = { Players = [{defaultPlayer with ID = 0}; {defaultPlayer with ID = 1}] }

    let init () = initModel, Cmd.none

    let updatePlayer pid action model =
        let nextPlayers = TunersTume.ListEx.update (fun p -> p.ID = pid) action model.Players
        { model with Players = nextPlayers }
        
    let updateLife pid point model =
        updatePlayer pid (fun p -> {p with Life = p.Life + point}) model
    
    let update msg model =
        match msg with
        | UpdateLife (pid, point) -> updateLife pid point model, Cmd.none 
        | Reset -> init ()
    
    let getPlayer id model = Seq.find (fun p -> p.ID = id) model.Players
    
    let view (model: Model) dispatch =
        View.ContentPage(
          content = View.StackLayout(padding = 20.0, verticalOptions = LayoutOptions.Center,
            children = [ 
                View.Label(text = sprintf "%d" (getPlayer 0 model).Life, horizontalOptions = LayoutOptions.Center, widthRequest=200.0, horizontalTextAlignment=TextAlignment.Center,
                           classId = "LifeCounterDigitStyle")               
                View.Button(text = "+5", command = (fun () -> dispatch (UpdateLife (0, 5))), horizontalOptions = LayoutOptions.Center)
                View.Button(text = "+1", command = (fun () -> dispatch (UpdateLife (0, 1))), horizontalOptions = LayoutOptions.Center)
                View.Button(text = "-5", command = (fun () -> dispatch (UpdateLife (0, -5))), horizontalOptions = LayoutOptions.Center)
                View.Button(text = "-1", command = (fun () -> dispatch (UpdateLife (0, -1))), horizontalOptions = LayoutOptions.Center)            
            ]))

    // Note, this declaration is needed if you enable LiveUpdate
    let program = Program.mkProgram init update view

type App () as app = 
    inherit Application ()

    let runner = 
        App.program
#if DEBUG
        |> Program.withConsoleTrace
#endif
        |> Program.runWithDynamicView app

#if DEBUG
    // Uncomment this line to enable live update in debug mode. 
    // See https://fsprojects.github.io/Fabulous/tools.html for further  instructions.
    //
    //do runner.EnableLiveUpdate()
#endif    

    // Uncomment this code to save the application state to app.Properties using Newtonsoft.Json
    // See https://fsprojects.github.io/Fabulous/models.html for further  instructions.
#if APPSAVE
    let modelId = "model"
    override __.OnSleep() = 

        let json = Newtonsoft.Json.JsonConvert.SerializeObject(runner.CurrentModel)
        Console.WriteLine("OnSleep: saving model into app.Properties, json = {0}", json)

        app.Properties.[modelId] <- json

    override __.OnResume() = 
        Console.WriteLine "OnResume: checking for model in app.Properties"
        try 
            match app.Properties.TryGetValue modelId with
            | true, (:? string as json) -> 

                Console.WriteLine("OnResume: restoring model from app.Properties, json = {0}", json)
                let model = Newtonsoft.Json.JsonConvert.DeserializeObject<App.Model>(json)

                Console.WriteLine("OnResume: restoring model from app.Properties, model = {0}", (sprintf "%0A" model))
                runner.SetCurrentModel (model, Cmd.none)

            | _ -> ()
        with ex -> 
            App.program.onError("Error while restoring model found in app.Properties", ex)

    override this.OnStart() = 
        Console.WriteLine "OnStart: using same logic as OnResume()"
        this.OnResume()
#endif


