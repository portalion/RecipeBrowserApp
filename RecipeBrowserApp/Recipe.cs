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

        public override string ToString() => JsonSerializer.Serialize(this);
    }
}
