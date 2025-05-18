/**
 * @name Identifies connections between call points including tag methods
 * @kind table
 * @id csharp/identify-call-point-connections
 */

import csharp
import callpoints
import callgraph
import externalfilter
import mappings

query predicate isSource(Callable c) { c instanceof CallPoints::InboundCallPoint }

query predicate isSink(Callable c) { c instanceof CallPoints::OutboundCallPoint }

string findDataflowName(Callable source, Method sink) {
  result =
    concat(Callable between, Method tagMethod, Call c |
      CallGraph::edgesWithoutTag+(source, between) and
      CallGraph::edgesWithoutTag+(between, sink) and
      Mappings::isDataflowTagMethod(tagMethod) and
      c.getTarget() = tagMethod and
      c.getAnArgument() = between
    |
      c.getArgument(0).getValue(), "|" order by CallGraph::distanceTo(source, between)
    )
}

string findDataflowMethodNameOrEmpty(Callable source, Method sink) {
  result =
    concat(Callable between, Method tagMethod |
      CallGraph::edgesWithoutTag+(source, between) and
      CallGraph::edgesWithoutTag+(between, sink) and
      Mappings::isDataflowTagMethod(tagMethod) and
      exists(Call c | c.getTarget() = tagMethod and c.getAnArgument() = between)
    |
      tagMethod.getName(), "|" order by CallGraph::distanceTo(source, between)
    )
}

from
  Callable source, Method sinkCaller, Method sink, string processname, string dataflowname,
  string dataflowmethodname, string internalcallfilepath, int internalcallstartline,
  int internalcallstartcolumn, int internalcallendline, int internalcallendcolumn,
  string processfilepath, int processstartline, int processstartcolumn, int processendline,
  int processendcolumn
where
  not ExternalFilter::isFilteredMethod(sink) and
  not Mappings::isDataflowTagMethod(source) and
  not Mappings::isDataflowTagMethod(sinkCaller) and
  not Mappings::isDataflowTagMethod(sink) and
  not exists(Method m | sink = m |
    m.getOverridee*().hasFullyQualifiedName("System.Object", "ToString") or
    m.getOverridee*().hasFullyQualifiedName("System.Object", "GetHashCode") or
    m.getOverridee*().hasFullyQualifiedName("System.Object", "Equals")
  ) and
  not sink instanceof UnboundGeneric and
  isSource(source) and
  isSink(sink) and
  CallGraph::edgesWithoutTag+(source, sink) and
  CallGraph::edgesWithoutTag*(source, sinkCaller) and
  CallGraph::isClosestMethod(sinkCaller, sink) and
  processname = Mappings::getProcessNameOrEmpty(source) and
  dataflowname = findDataflowName(source, sink) and
  dataflowmethodname = findDataflowMethodNameOrEmpty(source, sink) and
  sinkCaller
      .hasLocationInfo(internalcallfilepath, internalcallstartline, internalcallstartcolumn,
        internalcallendline, internalcallendcolumn) and
  source
      .getLocation()
      .hasLocationInfo(processfilepath, processstartline, processstartcolumn, processendline,
        processendcolumn)
select source.getFullyQualifiedNameDebug() as entrypoint,
  sinkCaller.getFullyQualifiedNameDebug() as internalcall,
  sink.getFullyQualifiedNameDebug() as externalcall, processname, dataflowname, dataflowmethodname,
  internalcallfilepath, internalcallstartline, internalcallstartcolumn, internalcallendline,
  internalcallendcolumn, processfilepath, processstartline, processstartcolumn, processendline, processendcolumn
