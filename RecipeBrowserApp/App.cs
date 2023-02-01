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
        private IEnumerable<Recipe> recipes = Enumerable.Empty<Recipe>();

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
                        EditCategories(recipe);
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
        private void EditCategories(Recipe recipe)
        {
            var tmp = recipe.Categories.ToList();
            while (true)
            {
                AnsiConsole.Clear();
                var choice = AnsiConsole.Prompt(new SelectionPrompt<string>().AddChoices(new string[]
                {
                    "Add",
                    "Remove",
                    "Nothing"
                }).WrapAround(true));
                if(!recipe.Categories.Any())
                switch (choice)
                {
                    case "Add":
                        tmp.Add(AnsiConsole.Ask<string>("What category you want to add?"));                 
                        break;
                    case "Remove":
                        if (!recipes.Any()) break;
                        tmp.Remove(AnsiConsole.Prompt(new SelectionPrompt<string>()
                            .Title("What category you want to remove?").AddChoices(tmp)));
                        break;
                    case "Nothing":
                        recipe.Categories = tmp;
                        return;
                }
            }
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
            if (!recipes.Any()) return;
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
        public void AddRecipe()
        {
            Recipe recipe = new Recipe();
            var askForSetting = (string text, string whatSet) =>
            {
                string toSet;
                do
                {
                    AnsiConsole.Clear();
                    toSet = AnsiConsole.Ask<string>(text);
                } while (!AnsiConsole.Confirm($"Set {whatSet} to: {toSet}?"));
                AnsiConsole.Clear();
                return toSet;
            };

            recipe.Title = askForSetting("How to name new recipe?", "title");
            recipe.Ingredients = askForSetting("What ingredients do you need?", "ingredients");
            recipe.Instructions = askForSetting("Tell me instructions on how to make that recipe: ", "instrustions");
            recipe.Categories = Enumerable.Empty<string>();
            EditCategories(recipe);
            if (AnsiConsole.Confirm($"Should i create new recipe called: {recipe.Title}"))
                recipes = recipes.Append(recipe);
        }
        public void Run()
        {
            bool running = true;

            (Action action, string name)[] availableChoices = new (Action action, string name)[]
                {
                    (ListAllRecipes, "List all recipes"),
                    (EditRecipes, "Edit recipes"),
                    (AddRecipe, "Add new recipe"),
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
