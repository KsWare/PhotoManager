using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace KsWare.PhotoManager.Common
{
	public class DebugCompositionContainer : CompositionContainer
	{
		private static readonly ThreadLocal<Stack<ExportStackEntry>> _stack =
			new ThreadLocal<Stack<ExportStackEntry>>(() => new Stack<ExportStackEntry>());

		private static readonly ThreadLocal<ExportStackEntry> _lastExport = new ThreadLocal<ExportStackEntry>();

		public DebugCompositionContainer(ComposablePartCatalog catalog, params ExportProvider[] providers) : base(
			catalog, providers)
		{
		}

		protected override IEnumerable<Export> GetExportsCore(ImportDefinition definition,
			AtomicComposition atomicComposition)
		{
			var level = _stack.Value.Count;
			var parent = _stack.Value.Count > 0 ? _stack.Value.Peek() : null;
			var entry = new ExportStackEntry(parent, definition, atomicComposition);
			if (parent == null) _lastExport.Value = entry;
			_stack.Value.Push(entry);
			var exports = base.GetExportsCore(definition, atomicComposition);
			_stack.Value.Pop();
			entry.SetReturnValue(exports);
			Debug.WriteLine($"Container.GetExportsCore <{level}> {definition.ContractName} {(definition.IsPrerequisite ? "Prerequisite" : "")} => {entry.Success}");
			if (entry.Success == false)
			{
				var e = entry;
				while (e.Parent!=null)
				{
					e.Parent.FailedChild = entry;
					e = e.Parent;
				}
			}

			return exports;
		}

		internal class ExportStackEntry
		{
			private readonly ImportDefinition _importDefinition;
			private readonly AtomicComposition _atomicComposition;

			public ExportStackEntry(ExportStackEntry parent, ImportDefinition importDefinition,
				AtomicComposition atomicComposition)
			{
				_importDefinition = importDefinition;
				_atomicComposition = atomicComposition;
				Parent = parent;
			}

			public string ContractName => _importDefinition.ContractName;
			public bool Success { get; private set; }

			public ExportStackEntry Parent { get; private set; }
			public ExportStackEntry FailedChild { get; set; }

			public void SetReturnValue(IEnumerable<Export> exports)
			{
				Success = exports.Any();
			}
		}

		public string GetFailedExportsTrace()
		{
			var sb=new StringBuilder();
			var e = _lastExport.Value;
			var level = 0;
			while (e != null)
			{
				if (sb.Length > 0) sb.Append("\n");
				sb.Append(new string(' ', level * 4));
				sb.Append(level==0 ? "    ": "--> ");
				sb.Append(e.ContractName);
				e = e.FailedChild;
				level++;
			}

			return sb.ToString();
		}
	}
}