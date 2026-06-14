#!/usr/bin/dotnet run
#:package Microsoft.Extensions.AI@10.4.1
#:package Microsoft.Extensions.AI.OpenAI@10.4.1
#:package Microsoft.Agents.AI.OpenAI@1.1.0
using System.ClientModel;
using System.ComponentModel;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;

// Tool Function: Random Focus Area Generator
// Returns a random muscle group or training style for the agent to build a session around
[Description("Provides a random workout focus area, such as a muscle group or training style.")]
static string GetRandomWorkoutFocus()
{
    var focusAreas = new List<string>
    {
        "Chest & Triceps",
        "Back & Biceps",
        "Legs & Glutes",
        "Shoulders & Core",
        "Full Body HIIT",
        "Upper Body Strength",
        "Lower Body Endurance",
        "Core & Mobility",
        "Push Day",
        "Pull Day"
    };

    var random = new Random();
    int index = random.Next(focusAreas.Count);
    return focusAreas[index];
}

// Extract configuration from environment variables
var github_endpoint = Environment.GetEnvironmentVariable("GH_ENDPOINT") ?? "https://models.github.ai/inference";
var github_model_id = Environment.GetEnvironmentVariable("GH_MODEL_ID") ?? "openai/gpt-5-mini";
var github_token = Environment.GetEnvironmentVariable("GH_TOKEN") ?? throw new InvalidOperationException("GH_TOKEN is not set.");

// Configure OpenAI client to point at GitHub Models
var openAIOptions = new OpenAIClientOptions()
{
    Endpoint = new Uri(github_endpoint)
};

var openAIClient = new OpenAIClient(new ApiKeyCredential(github_token), openAIOptions);

// Create AI Agent with Personal Trainer Capabilities
// The agent uses GetRandomWorkoutFocus to determine today's training focus,
// then builds a full session plan around it
AIAgent agent = openAIClient
    .GetChatClient(github_model_id)
    .AsAIAgent(
        instructions: "You are an enthusiastic personal trainer AI that designs structured workout sessions. " +
                      "When asked to plan a workout, use your tool to pick a focus area, then provide a warm-up, " +
                      "4-6 exercises with sets/reps/rest times, and a cool-down. Keep it practical and motivating.",
        tools: [AIFunctionFactory.Create(GetRandomWorkoutFocus)]
    );

// Run the agent with streaming so the session plan appears in real time
await foreach (var update in agent.RunStreamingAsync("Plan me a workout session for today"))
{
    await Task.Delay(10);
    Console.Write(update);
}
