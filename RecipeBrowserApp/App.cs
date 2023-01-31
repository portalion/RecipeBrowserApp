using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using Spectre.Console;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace RecipeBrowserApp
{
    internal class App
    {
        private string appPath;
        private string recipesPath;
        private IEnumerable<Recipe> recipes;

        public App()
        {
            appPath = Environment.CurrentDirectory;
            recipesPath = Path.Combine(appPath, "Data");
            if (!Directory.Exists(recipesPath)) Directory.CreateDirectory(recipesPath);
            recipesPath = Path.Combine(recipesPath, "Recipes.json");
            Init();
        }
        public void Init()  
        {
            if (File.Exists(recipesPath))
                recipes = JsonSerializer.Deserialize<IEnumerable<Recipe>>(File.ReadAllText(recipesPath)) ?? Enumerable.Empty<Recipe>();
            else recipes = Enumerable.Empty<Recipe>();
        }
        public void SaveRecipes()
        {
            using (FileStream toSave = File.Open(recipesPath, FileMode.Create, FileAccess.Write))
                JsonSerializer.Serialize(toSave, recipes);           
        }

        public void ListAllRecipes()
        {
            while (true)
            {
                AnsiConsole.Clear();
                if (recipes is null) recipes = Enumerable.Empty<Recipe>();
                if (!recipes.Any())
                {
                    AnsiConsole.WriteLine("There are no recipes");
                    if (AnsiConsole.Confirm("Want to go back? ")) return;
                    continue;
                }

                Table view = new Table();
                view.Expand();
                view.AddColumns("Title", "Ingredients", "Instructions", "Categories");
                foreach (var recipe in recipes)
                    view.AddRow(recipe.Title, recipe.Ingredients, recipe.Instructions, String.Join('\n', recipe.Categories));
                AnsiConsole.Write(view);
                if (AnsiConsole.Confirm("Want to go back? ")) break;
            }
        }

        public void Run()
        {
            bool running = true;

            (Action action, string name)[] availableChoices = new (Action action, string name)[]
                {
                    (ListAllRecipes, "List all recipes"),
                    (()=>{running = false; }, "Exit"),
                };

            var selectionPrompt = new SelectionPrompt<Action>()
                .Title("What you want to do?").PageSize(5)
                .MoreChoicesText("[grey](Move up and down to reveal more fruits)[/]");

            foreach (var choice in availableChoices)
                selectionPrompt.AddChoice(choice.action);

            selectionPrompt.UseConverter(action => Array.Find(availableChoices, val => val.action == action).name);

            while(running)
            {
                AnsiConsole.Clear();
                var choice = AnsiConsole.Prompt(selectionPrompt);
                choice();
            }
            SaveRecipes();
        }

    }
}
