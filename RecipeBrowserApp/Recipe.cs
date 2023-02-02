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
        public bool IsEmpty()
        {
            return string.IsNullOrEmpty(Title) && string.IsNullOrEmpty(Ingredients) 
                && string.IsNullOrEmpty(Instructions) && !Categories.Any();
        }
        public override string ToString() => JsonSerializer.Serialize(this);
    }
}
