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

        private void EditRecipe(Recipe recipe)
        {
            while(true)
            {
                AnsiConsole.Clear();
                var choice = AnsiConsole.Prompt(new SelectionPrompt<string>().Title("What you want to edit?").WrapAround(true)
                    .AddChoices(new string[]
                    {
                        "Title",
                        "Ingredients",
                        "Instructions",
                        "Categories",
                        "Remove",
                        "Nothing"
                    }));
                if (choice == "Nothing") break;
                string toReplace = "";
                if (choice != "Remove" && choice != "Categories") toReplace = AnsiConsole.Ask<string>($"Change {choice} to: ");
                switch (choice)
                {
                    case "Title":
                        recipe.Title = toReplace;
                        break;
                    case "Ingredients":
                        recipe.Ingredients = toReplace;
                        break;
                    case "Instructions":
                        recipe.Instructions = toReplace;
                        break;
                    case "Categories":
                        AddNewCategories(recipe);
                        break;
                    case "Remove":
                        if (AnsiConsole.Confirm($"Really want to delete recipe with title: {recipe.Title}", false))
                        {
                            recipes = recipes.Except(new Recipe[] { recipe });
                            return;
                        }
                        break;
                }
            }
        }
        public void AddNewCategories(Recipe recipe)
        {
            var tmp = recipe.Categories.ToList();
            while(AnsiConsole.Confirm("Do you want to add more categories?"))
            {
                tmp.Add(AnsiConsole.Ask<string>("What category you want to add?"));
                AnsiConsole.Clear();
            }
            recipe.Categories = tmp;
        }

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

        public void EditRecipes()
        {
            while(true)
            {
                if (AnsiConsole.Confirm("Want to go back?", false)) break;
                var selectionPrompt = new SelectionPrompt<Recipe>().Title("Which recipe you want to edit?").PageSize(5)
                .MoreChoicesText("[grey](Move up and down to reveal more recipes)[/]"); ;

                foreach(var recipe in recipes)
                    selectionPrompt.AddChoice(recipe);
                selectionPrompt.Converter = recipe => recipe.Title;

                EditRecipe(AnsiConsole.Prompt(selectionPrompt));
            }
        }

        public void Run()
        {
            bool running = true;

            (Action action, string name)[] availableChoices = new (Action action, string name)[]
                {
                    (ListAllRecipes, "List all recipes"),
                    (EditRecipes, "Edit recipes"),
                    (()=>{running = false; }, "Exit"),
                };

            var selectionPrompt = new SelectionPrompt<Action>()
                .WrapAround(true)
                .Title("What you want to do?")
                .MoreChoicesText("[grey](Move up and down to reveal more options)[/]");

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
