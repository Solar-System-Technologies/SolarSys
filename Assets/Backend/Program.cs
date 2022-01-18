﻿using System;

namespace Backend
{
    class Program
    {
        public static void Main()
        {
            using (var database = new DatabaseView("bolt://localhost:7687", "neo4j", "password"))
            {
                GraphNode node = database.FindNodeByGuid(Guid.Parse("b22f0d72-cf45-481d-b697-80c0350341b9"));
                Console.WriteLine(node.Text);
            }
        }
    }
}
