// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;
using OpenAI.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Scripting;

namespace OpenAI.Threads
{
    /// <summary>
    /// Create a run on a thread.
    /// </summary>
    [Preserve]
    public sealed class CreateRunRequest
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="assistantId">
        /// The ID of the assistant used for execution of this run.
        /// </param>
        /// <param name="model">
        /// The model that the assistant used for this run.
        /// </param>
        /// <param name="instructions">
        /// The instructions that the assistant used for this run.
        /// </param>
        /// <param name="additionalInstructions">
        /// Appends additional instructions at the end of the instructions for the run.
        /// This is useful for modifying the behavior on a per-run basis without overriding other instructions.
        /// </param>
        /// <param name="additionalMessages">
        /// Adds additional messages to the thread before creating the run.
        /// </param>
        /// <param name="tools">
        /// The list of tools that the assistant used for this run.
        /// </param>
        /// <param name="metadata">
        /// Set of 16 key-value pairs that can be attached to an object.
        /// This can be useful for storing additional information about the object in a structured format.
        /// Keys can be a maximum of 64 characters long and values can be a maximum of 512 characters long.
        /// </param>
        /// <param name="temperature">
        /// What sampling temperature to use, between 0 and 2. Higher values like 0.8 will make the output
        /// more random, while lower values like 0.2 will make it more focused and deterministic.
        /// When null the default temperature (1) will be used.
        /// </param>
        /// <param name="topP">
        /// An alternative to sampling with temperature, called nucleus sampling,
        /// where the model considers the results of the tokens with top_p probability mass.
        /// So 0.1 means only the tokens comprising the top 10% probability mass are considered.
        /// We generally recommend altering this or temperature but not both.
        /// </param>
        /// <param name="maxPromptTokens">
        /// The maximum number of prompt tokens that may be used over the course of the run.
        /// The run will make a best effort to use only the number of prompt tokens specified,
        /// across multiple turns of the run. If the run exceeds the number of prompt tokens specified,
        /// the run will end with status 'incomplete'. See 'incomplete_details' for more info.
        /// </param>
        /// <param name="maxCompletionTokens">
        /// The maximum number of completion tokens that may be used over the course of the run.
        /// The run will make a best effort to use only the number of completion tokens specified,
        /// across multiple turns of the run. If the run exceeds the number of completion tokens specified,
        /// the run will end with status 'incomplete'. See 'incomplete_details' for more info.
        /// </param>
        /// <param name="truncationStrategy">
        /// Controls for how a thread will be truncated prior to the run.
        /// Use this to control the initial context window of the run.
        /// </param>
        /// <param name="toolChoice">
        /// Controls which (if any) tool is called by the model.
        /// none means the model will not call any tools and instead generates a message.
        /// auto is the default value and means the model can pick between generating a message or calling one or more tools.
        /// required means the model must call one or more tools before responding to the user.
        /// Specifying a particular tool like {"type": "file_search"} or {"type": "function", "function": {"name": "my_function"}}
        /// forces the model to call that tool.
        /// </param>
        /// <param name="parallelToolCalls">
        /// Whether to enable parallel function calling during tool use.
        /// </param>
        /// <param name="jsonSchema">
        /// The <see cref="JsonSchema"/> to use for structured JSON outputs.<br/>
        /// <see href="https://platform.openai.com/docs/guides/structured-outputs"/><br/>
        /// <see href="https://json-schema.org/overview/what-is-jsonschema"/>
        /// </param>
        /// <param name="responseFormat">
        /// An object specifying the format that the model must output.
        /// Setting to <see cref="ChatResponseFormat.Json"/> or <see cref="ChatResponseFormat.JsonSchema"/> enables JSON mode,
        /// which guarantees the message the model generates is valid JSON.<br/>
        /// Important: When using JSON mode, you must also instruct the model to produce JSON yourself via a system or user message.
        /// Without this, the model may generate an unending stream of whitespace until the generation reaches the token limit,
        /// resulting in a long-running and seemingly "stuck" request. Also note that the message content may be partially cut off if finish_reason="length",
        /// which indicates the generation exceeded max_tokens or the conversation exceeded the max context length.
        /// </param>
        [Preserve]
        public CreateRunRequest(
            string assistantId,
            string model = null,
            string instructions = null,
            string additionalInstructions = null,
            IEnumerable<Message> additionalMessages = null,
            IEnumerable<Tool> tools = null,
            IReadOnlyDictionary<string, string> metadata = null,
            double? temperature = null,
            double? topP = null,
            int? maxPromptTokens = null,
            int? maxCompletionTokens = null,
            TruncationStrategy truncationStrategy = null,
            string toolChoice = null,
            bool? parallelToolCalls = null,
            JsonSchema jsonSchema = null,
            ChatResponseFormat responseFormat = ChatResponseFormat.Text)
        {
            AssistantId = assistantId;
            Model = model;
            Instructions = instructions;
            AdditionalInstructions = additionalInstructions;
            AdditionalMessages = additionalMessages?.ToList();

            var toolList = tools?.ToList();

            if (toolList is { Count: > 0 })
            {
                if (string.IsNullOrWhiteSpace(toolChoice))
                {
                    ToolChoice = "auto";
                }
                else
                {
                    if (!toolChoice.Equals("none") &&
                        !toolChoice.Equals("required") &&
                        !toolChoice.Equals("auto"))
                    {
                        var tool = toolList.FirstOrDefault(t => t.Function.Name.Contains(toolChoice)) ??
                                   throw new ArgumentException($"The specified tool choice '{toolChoice}' was not found in the list of tools");
                        ToolChoice = new { type = "function", function = new { name = tool.Function.Name } };
                    }
                    else
                    {
                        ToolChoice = toolChoice;
                    }
                }

                foreach (var tool in toolList)
                {
                    if (tool?.Function?.Arguments != null)
                    {
                        // just in case clear any lingering func args.
                        tool.Function.Arguments = null;
                    }
                }
            }

            Tools = toolList?.ToList();
            Metadata = metadata;
            Temperature = temperature;
            TopP = topP;
            MaxPromptTokens = maxPromptTokens;
            MaxCompletionTokens = maxCompletionTokens;
            TruncationStrategy = truncationStrategy;
            ParallelToolCalls = parallelToolCalls;

            if (jsonSchema != null)
            {
                ResponseFormatObject = jsonSchema;
            }
            else
            {
                ResponseFormatObject = responseFormat;
            }
        }

        /// <summary>
        /// The ID of the assistant used for execution of this run.
        /// </summary>
        [Preserve]
        [JsonProperty("assistant_id")]
        public string AssistantId { get; internal set; }

        /// <summary>
        /// The model that the assistant used for this run.
        /// </summary>
        [Preserve]
        [JsonProperty("model")]
        public string Model { get; }

        /// <summary>
        /// The instructions that the assistant used for this run.
        /// </summary>
        [Preserve]
        [JsonProperty("instructions")]
        public string Instructions { get; }

        /// <summary>
        /// Appends additional instructions at the end of the instructions for the run.
        /// This is useful for modifying the behavior on a per-run basis without overriding other instructions.
        /// </summary>
        [Preserve]
        [JsonProperty("additional_instructions")]
        public string AdditionalInstructions { get; }

        /// <summary>
        /// Adds additional messages to the thread before creating the run.
        /// </summary>
        [Preserve]
        [JsonProperty("additional_messages")]
        public IReadOnlyList<Message> AdditionalMessages { get; }

        /// <summary>
        /// The list of tools that the assistant used for this run.
        /// </summary>
        [Preserve]
        [JsonProperty("tools")]
        public IReadOnlyList<Tool> Tools { get; }

        /// <summary>
        /// Set of 16 key-value pairs that can be attached to an object.
        /// This can be useful for storing additional information about the object in a structured format.
        /// Keys can be a maximum of 64 characters long and values can be a maximum of 512 characters long.
        /// </summary>
        [Preserve]
        [JsonProperty("metadata")]
        public IReadOnlyDictionary<string, string> Metadata { get; }

        /// <summary>
        /// What sampling temperature to use, between 0 and 2. Higher values like 0.8 will make the output
        /// more random, while lower values like 0.2 will make it more focused and deterministic.
        /// When null the default temperature (1) will be used.
        /// </summary>
        [Preserve]
        [JsonProperty("temperature")]
        public double? Temperature { get; }

        /// <summary>
        /// An alternative to sampling with temperature, called nucleus sampling,
        /// where the model considers the results of the tokens with top_p probability mass.
        /// So 0.1 means only the tokens comprising the top 10% probability mass are considered.
        /// We generally recommend altering this or temperature but not both.
        /// </summary>
        [Preserve]
        [JsonProperty("top_p")]
        public double? TopP { get; }

        /// <summary>
        /// If true, returns a stream of events that happen during the Run as server-sent events,
        /// terminating when the Run enters a terminal state with a 'data: [DONE]' message.
        /// </summary>
        [Preserve]
        [JsonProperty("stream")]
        public bool Stream { get; internal set; }

        /// <summary>
        /// The maximum number of prompt tokens that may be used over the course of the run.
        /// The run will make a best effort to use only the number of prompt tokens specified,
        /// across multiple turns of the run. If the run exceeds the number of prompt tokens specified,
        /// the run will end with status 'incomplete'. See 'incomplete_details' for more info.
        /// </summary>
        [Preserve]
        [JsonProperty("max_prompt_tokens")]
        public int? MaxPromptTokens { get; }

        /// <summary>
        /// The maximum number of completion tokens that may be used over the course of the run.
        /// The run will make a best effort to use only the number of completion tokens specified,
        /// across multiple turns of the run. If the run exceeds the number of completion tokens specified,
        /// the run will end with status 'incomplete'. See 'incomplete_details' for more info.
        /// </summary>
        [Preserve]
        [JsonProperty("max_completion_tokens")]
        public int? MaxCompletionTokens { get; }

        /// <summary>
        /// Controls for how a thread will be truncated prior to the run.
        /// Use this to control the initial context window of the run.
        /// </summary>
        [Preserve]
        [JsonProperty("truncation_strategy")]
        public TruncationStrategy TruncationStrategy { get; }

        /// <summary>
        /// Controls which (if any) tool is called by the model.
        /// none means the model will not call any tools and instead generates a message.
        /// auto is the default value and means the model can pick between generating a message or calling one or more tools.
        /// required means the model must call one or more tools before responding to the user.
        /// Specifying a particular tool like {"type": "file_search"} or {"type": "function", "function": {"name": "my_function"}}
        /// forces the model to call that tool.
        /// </summary>
        [Preserve]
        [JsonProperty("tool_choice")]
        public object ToolChoice { get; }

        /// <summary>
        /// Whether to enable parallel function calling during tool use.
        /// </summary>
        [Preserve]
        [JsonProperty("parallel_tool_calls")]
        public bool? ParallelToolCalls { get; }

        /// <summary>
        /// An object specifying the format that the model must output.
        /// Setting to <see cref="ChatResponseFormat.Json"/> or <see cref="ChatResponseFormat.JsonSchema"/> enables JSON mode,
        /// which guarantees the message the model generates is valid JSON.
        /// </summary>
        /// <remarks>
        /// Important: When using JSON mode you must still instruct the model to produce JSON yourself via some conversation message,
        /// for example via your system message. If you don't do this, the model may generate an unending stream of
        /// whitespace until the generation reaches the token limit, which may take a lot of time and give the appearance
        /// of a "stuck" request. Also note that the message content may be partial (i.e. cut off) if finish_reason="length",
        /// which indicates the generation exceeded max_tokens or the conversation exceeded the max context length.
        /// </remarks>
        [Preserve]
        [JsonConverter(typeof(ResponseFormatConverter))]
        [JsonProperty("response_format", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public ResponseFormatObject ResponseFormatObject { get; internal set; }

        [Preserve]
        [JsonIgnore]
        public ChatResponseFormat ResponseFormat => ResponseFormatObject ?? ChatResponseFormat.Auto;
    }
}
