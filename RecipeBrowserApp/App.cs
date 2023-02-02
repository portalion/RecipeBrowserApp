using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using Spectre.Console;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using Spectre.Console.Rendering;
using System.Xml.Serialization;

namespace RecipeBrowserApp
{
    internal class App //Simple class for whole app (that console app isnt so hard to implement to break up
    {
        private string appPath;
        private string recipesPath;
        private IEnumerable<Recipe> recipes = Enumerable.Empty<Recipe>();

        private void EditRecipe(Recipe recipe)
        {
            while (true)
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

        private Panel GetRecipeRenderablePanel(Recipe recipe)
        {
            List<IRenderable> panelContent = new List<IRenderable>();
            var AddWithBottomBorder = (string text, string ruleText) =>
            {
                panelContent.Add(new Text(text).Centered());
                panelContent.Add(new Rule(ruleText));
            };
            AddWithBottomBorder(recipe.Ingredients, "Instructions");
            AddWithBottomBorder(recipe.Instructions, "Categories");

            panelContent.Add(new Text(string.Join('\n', recipe.Categories)).Centered());

            var result = new Panel(new Rows(panelContent)).Header(recipe.Title, Justify.Center);
            result.Width = 40;
            return result;
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
            //That function only reads data from Recipes.json file
            if (File.Exists(recipesPath))
                recipes = JsonSerializer.Deserialize<IEnumerable<Recipe>>(File.ReadAllText(recipesPath)) ?? Enumerable.Empty<Recipe>();
            else recipes = Enumerable.Empty<Recipe>();
        }
        public void SaveRecipes()
        {
            //Function only for saving to recipes.json file
            using (FileStream toSave = File.Open(recipesPath, FileMode.Create, FileAccess.Write))
                JsonSerializer.Serialize(toSave, recipes);
        }
        public void ListAllRecipes()
        {
            AnsiConsole.Clear();
            if (!recipes.Any())
            {
                AnsiConsole.WriteLine("There are no recipes");
                Console.ReadKey();
                return;
            }

            //Make table with all informations about recipe, after that wait for key
            Table allRecipes = new Table().AddColumns("Title", "Ingredients", "Instructions", "Categories").Expand().MinimalBorder();
            foreach (var recipe in recipes)
            {
                allRecipes.AddRow(recipe.Title, recipe.Ingredients, recipe.Instructions,
                   string.Join('\n', recipe.Categories));
                if (recipes.Last() != recipe) allRecipes.AddRow(new Rule(), new Rule(), new Rule(), new Rule());
            }

            AnsiConsole.Write(allRecipes);
            AnsiConsole.Write("Press any key to continue");
            Console.ReadKey();

        }
        public void EditRecipes() //Use recipe editor only here
        {
            if (!recipes.Any()) return;
            while (true)
            {
                if (AnsiConsole.Confirm("Want to go back?", false)) break;
                var selectionPrompt = new SelectionPrompt<Recipe>().Title("Which recipe you want to edit?").PageSize(5)
                .MoreChoicesText("[grey](Move up and down to reveal more recipes)[/]"); ;

                foreach (var recipe in recipes)
                    selectionPrompt.AddChoice(recipe);
                selectionPrompt.Converter = recipe => recipe.Title;

                EditRecipe(AnsiConsole.Prompt(selectionPrompt));
            }
        }
        public void AddRecipe() //Change it to recipe editor and add new AddRecipe function
        {
            //Recipe which will be added (or no)
            Recipe newRecipe = new Recipe();
            bool waitingToCreate = true;

            //Here add new Functionalities to Recipe Editor
            (Action action, string name)[] availableChoices = new (Action action, string name)[]
            {
                (() => newRecipe.Title = AnsiConsole.Ask<string>("How to edit title?"), "Edit title"),
                (() => newRecipe.Title = string.Empty, "Remove title"),

                (() => newRecipe.Ingredients = AnsiConsole.Ask<string>("How to edit ingredients?(if you want newline type \\n)")
                    .Replace("\\n", "\n") + '\n', "Edit ingredients"),
                (() => newRecipe.Ingredients += AnsiConsole.Ask<string>("What line should i add?") + '\n', "Add line to ingredients"),
                (() => newRecipe.Ingredients = string.Empty, "Clear ingredients"),

                (() => newRecipe.Instructions = AnsiConsole.Ask<string>("How to edit Instructions?(if you want newline type \\n)")
                    .Replace("\\n", "\n") + '\n', "Edit Instructions"),
                (() => newRecipe.Instructions += AnsiConsole.Ask<string>("What line should i add?") + '\n', "Add line to Instructions"),
                (() => newRecipe.Instructions = string.Empty, "Clear Instructions"),

                (() => EditCategories(newRecipe), "Edit Categories"),
                (() => {waitingToCreate = false; recipes = recipes.Append(newRecipe); }, "Save and exit"),
                (() => waitingToCreate = false, "Cancel")
            };

            //This prompt is made of Actions, we get string that will be displayed from available
            //choices table. (thats what UseConverter lambda does)
            var selectionPrompt = new SelectionPrompt<Action>()
                .WrapAround(true)
                .PageSize(15)
                .Title("Welcome in Recipe Creator.\nWhat you want to do?")
                .UseConverter(action => Array.Find(availableChoices, val => val.action == action).name);

            foreach (var choice in availableChoices)
                selectionPrompt.AddChoice(choice.action);

            while (waitingToCreate)
            {
                AnsiConsole.Clear();
                if (!newRecipe.IsEmpty()) AnsiConsole.Write(Align.Center(GetRecipeRenderablePanel(newRecipe)));
                var choice = AnsiConsole.Prompt(selectionPrompt);
                choice();
            }
        }
        public void Run() //Add remove recipe option
        {
            //Variable for managing loop
            bool running = true;

            //Here add new options to main menu, format: (Function, Option String)
            (Action action, string name)[] availableChoices = new (Action action, string name)[]
                {
                    (ListAllRecipes, "List all recipes"),
                    (EditRecipes, "Edit recipes"),
                    (AddRecipe, "Add new recipe"),
                    (()=>{running = false; }, "Exit"),
                };

            //This prompt is made of Actions, we get string that will be displayed from available
            //choices table. (thats what UseConverter lambda does)
            var selectionPrompt = new SelectionPrompt<Action>()
                .WrapAround(true)
                .Title("What you want to do?")
                .UseConverter(action => Array.Find(availableChoices, val => val.action == action).name);

            foreach (var choice in availableChoices)
                selectionPrompt.AddChoice(choice.action);

            while (running)
            {
                AnsiConsole.Clear();
                AnsiConsole.Prompt(selectionPrompt)();
            }
            //Save recipes to JSON after ending main loop
            SaveRecipes();
        }

    }
}
