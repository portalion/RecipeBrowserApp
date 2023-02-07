using System;
using System.Collections.Generic;
using System.Text.Json;

namespace RecipeBrowserApp
{
    internal class Recipe
    {
        public string Title { get; set; }
        public string Ingredients { get; set; }
        public string Instructions { get; set; }
        public IEnumerable<string> Categories { get; set; }

        public Recipe()
        {
            Title = string.Empty;
            Ingredients = string.Empty;
            Instructions = string.Empty;
            Categories = Enumerable.Empty<string>();
        }

        public Recipe Copy()
        {
            Recipe recipe = new Recipe();
            recipe.Title = Title;
            recipe.Instructions = Instructions;
            recipe.Ingredients = Ingredients;
            List<string> categoriesList = new List<string>();
            
            foreach(var category in Categories) 
                categoriesList.Add(category);
            return recipe;
        }

        public bool IsEmpty()
        {
            return string.IsNullOrEmpty(Title) && string.IsNullOrEmpty(Ingredients) 
                && string.IsNullOrEmpty(Instructions) && !Categories.Any();
        }
        public override string ToString() => JsonSerializer.Serialize(this);
    }
}
