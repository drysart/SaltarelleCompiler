using System.Runtime.CompilerServices;

namespace NodeJS.FSModule {
	[Imported]
	[NamedValues]
	[IgnoreNamespace]
	public enum SymlinkType {
		Dir,
		File,
		Junction,
	}
}