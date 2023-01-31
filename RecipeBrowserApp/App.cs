using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using Spectre.Console;
using System.Threading.Tasks;

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
            if (recipes is null) return;
            foreach (var recipe in recipes)
                AnsiConsole.WriteLine(recipe.ToString());       
        }

        public void Run()
        {
            ListAllRecipes();
            SaveRecipes();
        }

    }
}
