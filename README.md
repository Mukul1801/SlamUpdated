> Entering new SQL Agent Executor chain...
---------------------------------------------------------------------------
KeyError                                  Traceback (most recent call last)
Cell In[32], line 2
      1 question = "What iwas the most energy efficient plant in 2024?"
----> 2 response  = agent_trigger(question)

Cell In[31], line 29, in agent_trigger(question)
     11 dbr_agent = create_sql_agent(
     12     llm=llm_instance,
     13     toolkit=toolkit,\
   (...)
     17     verbose=True
     18     )
     20 sql_dbr_agent = create_sql_agent(
     21             llm=llm_instance,
     22             toolkit=toolkit,
   (...)
     27             max_iterations=5
     28         )
---> 29 return (dbr_agent.invoke(question),sql_dbr_agent.invoke(question))

File /anaconda/envs/azureml_py38/lib/python3.10/site-packages/langchain/chains/base.py:167, in Chain.invoke(self, input, config, **kwargs)
    165 except BaseException as e:
    166     run_manager.on_chain_error(e)
--> 167     raise e
    168 run_manager.on_chain_end(outputs)
    170 if include_run_info:

File /anaconda/envs/azureml_py38/lib/python3.10/site-packages/langchain/chains/base.py:157, in Chain.invoke(self, input, config, **kwargs)
    154 try:
    155     self._validate_inputs(inputs)
    156     outputs = (
--> 157         self._call(inputs, run_manager=run_manager)
    158         if new_arg_supported
    159         else self._call(inputs)
    160     )
    162     final_outputs: dict[str, Any] = self.prep_outputs(
    163         inputs, outputs, return_only_outputs
    164     )
    165 except BaseException as e:

File /anaconda/envs/azureml_py38/lib/python3.10/site-packages/langchain/agents/agent.py:1620, in AgentExecutor._call(self, inputs, run_manager)
   1618 # We now enter the agent loop (until it returns something).
   1619 while self._should_continue(iterations, time_elapsed):
-> 1620     next_step_output = self._take_next_step(
   1621         name_to_tool_map,
   1622         color_mapping,
   1623         inputs,
   1624         intermediate_steps,
   1625         run_manager=run_manager,
   1626     )
   1627     if isinstance(next_step_output, AgentFinish):
   1628         return self._return(
   1629             next_step_output, intermediate_steps, run_manager=run_manager
   1630         )

File /anaconda/envs/azureml_py38/lib/python3.10/site-packages/langchain/agents/agent.py:1326, in AgentExecutor._take_next_step(self, name_to_tool_map, color_mapping, inputs, intermediate_steps, run_manager)
   1317 def _take_next_step(
   1318     self,
   1319     name_to_tool_map: dict[str, BaseTool],
   (...)
   1323     run_manager: Optional[CallbackManagerForChainRun] = None,
   1324 ) -> Union[AgentFinish, list[tuple[AgentAction, str]]]:
   1325     return self._consume_next_step(
-> 1326         [
   1327             a
   1328             for a in self._iter_next_step(
   1329                 name_to_tool_map,
   1330                 color_mapping,
   1331                 inputs,
   1332                 intermediate_steps,
   1333                 run_manager,
   1334             )
   1335         ]
   1336     )

File /anaconda/envs/azureml_py38/lib/python3.10/site-packages/langchain/agents/agent.py:1326, in <listcomp>(.0)
   1317 def _take_next_step(
   1318     self,
   1319     name_to_tool_map: dict[str, BaseTool],
   (...)
   1323     run_manager: Optional[CallbackManagerForChainRun] = None,
   1324 ) -> Union[AgentFinish, list[tuple[AgentAction, str]]]:
   1325     return self._consume_next_step(
-> 1326         [
   1327             a
   1328             for a in self._iter_next_step(
   1329                 name_to_tool_map,
   1330                 color_mapping,
   1331                 inputs,
   1332                 intermediate_steps,
   1333                 run_manager,
   1334             )
   1335         ]
   1336     )

File /anaconda/envs/azureml_py38/lib/python3.10/site-packages/langchain/agents/agent.py:1354, in AgentExecutor._iter_next_step(self, name_to_tool_map, color_mapping, inputs, intermediate_steps, run_manager)
   1351     intermediate_steps = self._prepare_intermediate_steps(intermediate_steps)
   1353     # Call the LLM to see what to do.
-> 1354     output = self._action_agent.plan(
   1355         intermediate_steps,
   1356         callbacks=run_manager.get_child() if run_manager else None,
   1357         **inputs,
   1358     )
   1359 except OutputParserException as e:
   1360     if isinstance(self.handle_parsing_errors, bool):

File /anaconda/envs/azureml_py38/lib/python3.10/site-packages/langchain/agents/agent.py:577, in RunnableMultiActionAgent.plan(self, intermediate_steps, callbacks, **kwargs)
    569 final_output: Any = None
    570 if self.stream_runnable:
    571     # Use streaming to make sure that the underlying LLM is invoked in a
    572     # streaming
   (...)
    575     # Because the response from the plan is not a generator, we need to
    576     # accumulate the output into final output and return that.
--> 577     for chunk in self.runnable.stream(inputs, config={"callbacks": callbacks}):
    578         if final_output is None:
    579             final_output = chunk

File /anaconda/envs/azureml_py38/lib/python3.10/site-packages/langchain_core/runnables/base.py:3424, in RunnableSequence.stream(self, input, config, **kwargs)
   3417 @override
   3418 def stream(
   3419     self,
   (...)
   3422     **kwargs: Optional[Any],
   3423 ) -> Iterator[Output]:
-> 3424     yield from self.transform(iter([input]), config, **kwargs)

File /anaconda/envs/azureml_py38/lib/python3.10/site-packages/langchain_core/runnables/base.py:3410, in RunnableSequence.transform(self, input, config, **kwargs)
   3403 @override
   3404 def transform(
   3405     self,
   (...)
   3408     **kwargs: Optional[Any],
   3409 ) -> Iterator[Output]:
-> 3410     yield from self._transform_stream_with_config(
   3411         input,
   3412         self._transform,
   3413         patch_config(config, run_name=(config or {}).get("run_name") or self.name),
   3414         **kwargs,
   3415     )

File /anaconda/envs/azureml_py38/lib/python3.10/site-packages/langchain_core/runnables/base.py:2205, in Runnable._transform_stream_with_config(self, input, transformer, config, run_type, **kwargs)
   2203 try:
   2204     while True:
-> 2205         chunk: Output = context.run(next, iterator)
   2206         yield chunk
   2207         if final_output_supported:

File /anaconda/envs/azureml_py38/lib/python3.10/site-packages/langchain_core/runnables/base.py:3372, in RunnableSequence._transform(self, input, run_manager, config, **kwargs)
   3369     else:
   3370         final_pipeline = step.transform(final_pipeline, config)
-> 3372 yield from final_pipeline

File /anaconda/envs/azureml_py38/lib/python3.10/site-packages/langchain_core/runnables/base.py:1419, in Runnable.transform(self, input, config, **kwargs)
   1416 final: Input
   1417 got_first_val = False
-> 1419 for ichunk in input:
   1420     # The default implementation of transform is to buffer input and
   1421     # then call stream.
   1422     # It'll attempt to gather all input into a single chunk using
   1423     # the `+` operator.
   1424     # If the input is not addable, then we'll assume that we can
   1425     # only operate on the last chunk,
   1426     # and we'll iterate until we get to the last chunk.
   1427     if not got_first_val:
   1428         final = ichunk

File /anaconda/envs/azureml_py38/lib/python3.10/site-packages/langchain_core/runnables/base.py:5632, in RunnableBindingBase.transform(self, input, config, **kwargs)
   5625 @override
   5626 def transform(
   5627     self,
   (...)
   5630     **kwargs: Any,
   5631 ) -> Iterator[Output]:
-> 5632     yield from self.bound.transform(
   5633         input,
   5634         self._merge_configs(config),
   5635         **{**self.kwargs, **kwargs},
   5636     )

File /anaconda/envs/azureml_py38/lib/python3.10/site-packages/langchain_core/runnables/base.py:1419, in Runnable.transform(self, input, config, **kwargs)
   1416 final: Input
   1417 got_first_val = False
-> 1419 for ichunk in input:
   1420     # The default implementation of transform is to buffer input and
   1421     # then call stream.
   1422     # It'll attempt to gather all input into a single chunk using
   1423     # the `+` operator.
   1424     # If the input is not addable, then we'll assume that we can
   1425     # only operate on the last chunk,
   1426     # and we'll iterate until we get to the last chunk.
   1427     if not got_first_val:
   1428         final = ichunk

File /anaconda/envs/azureml_py38/lib/python3.10/site-packages/langchain_core/runnables/base.py:1437, in Runnable.transform(self, input, config, **kwargs)
   1434             final = ichunk
   1436 if got_first_val:
-> 1437     yield from self.stream(final, config, **kwargs)

File /anaconda/envs/azureml_py38/lib/python3.10/site-packages/langchain_core/runnables/base.py:1004, in Runnable.stream(self, input, config, **kwargs)
    986 def stream(
    987     self,
    988     input: Input,
    989     config: Optional[RunnableConfig] = None,
    990     **kwargs: Optional[Any],
    991 ) -> Iterator[Output]:
    992     """Default implementation of stream, which calls invoke.
    993 
    994     Subclasses should override this method if they support streaming output.
   (...)
   1002         The output of the Runnable.
   1003     """
-> 1004     yield self.invoke(input, config, **kwargs)

File /anaconda/envs/azureml_py38/lib/python3.10/site-packages/langchain_core/prompts/base.py:216, in BasePromptTemplate.invoke(self, input, config, **kwargs)
    214 if self.tags:
    215     config["tags"] = config["tags"] + self.tags
--> 216 return self._call_with_config(
    217     self._format_prompt_with_error_handling,
    218     input,
    219     config,
    220     run_type="prompt",
    221     serialized=self._serialized,
    222 )

File /anaconda/envs/azureml_py38/lib/python3.10/site-packages/langchain_core/runnables/base.py:1930, in Runnable._call_with_config(self, func, input, config, run_type, serialized, **kwargs)
   1926     child_config = patch_config(config, callbacks=run_manager.get_child())
   1927     with set_config_context(child_config) as context:
   1928         output = cast(
   1929             "Output",
-> 1930             context.run(
   1931                 call_func_with_variable_args,  # type: ignore[arg-type]
   1932                 func,
   1933                 input,
   1934                 config,
   1935                 run_manager,
   1936                 **kwargs,
   1937             ),
   1938         )
   1939 except BaseException as e:
   1940     run_manager.on_chain_error(e)

File /anaconda/envs/azureml_py38/lib/python3.10/site-packages/langchain_core/runnables/config.py:428, in call_func_with_variable_args(func, input, config, run_manager, **kwargs)
    426 if run_manager is not None and accepts_run_manager(func):
    427     kwargs["run_manager"] = run_manager
--> 428 return func(input, **kwargs)

File /anaconda/envs/azureml_py38/lib/python3.10/site-packages/langchain_core/prompts/base.py:189, in BasePromptTemplate._format_prompt_with_error_handling(self, inner_input)
    188 def _format_prompt_with_error_handling(self, inner_input: dict) -> PromptValue:
--> 189     _inner_input = self._validate_input(inner_input)
    190     return self.format_prompt(**_inner_input)

File /anaconda/envs/azureml_py38/lib/python3.10/site-packages/langchain_core/prompts/base.py:183, in BasePromptTemplate._validate_input(self, inner_input)
    177     example_key = missing.pop()
    178     msg += (
    179         f"\nNote: if you intended {{{example_key}}} to be part of the string"
    180         " and not a variable, please escape it with double curly braces like: "
    181         f"'{{{{{example_key}}}}}'."
    182     )
--> 183     raise KeyError(
    184         create_message(message=msg, error_code=ErrorCode.INVALID_PROMPT_INPUT)
    185     )
    186 return inner_input

KeyError: 'Input to ChatPromptTemplate is missing variables {\'"answer"\', \'\\n            "SQLQuery"\'}.  Expected: [\'\\n            "SQLQuery"\', \'"answer"\', \'agent_scratchpad\', \'input\'] Received: [\'input\', \'intermediate_steps\', \'agent_scratchpad\']\nNote: if you intended {"answer"} to be part of the string and not a variable, please escape it with double curly braces like: \'{{"answer"}}\'.\nFor troubleshooting, visit: https://python.langchain.com/docs/troubleshooting/errors/INVALID_PROMPT_INPUT '
