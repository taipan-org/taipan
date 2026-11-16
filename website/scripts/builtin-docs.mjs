// Writes src/Taipan.Compiler/Semantics/BuiltinDocs.cs, the built-ins' documentation editors show
// (SPEC §6, §16.3, M58), from the library reference's tables. Run it after editing one of those
// tables: `npm run builtin-docs` in website/. BuiltinDocsTests fails until the two agree.
//
// Each table row's first cell holds one or more `forms`, and its second cell their description.
// Methods are keyed by type and name (str.strip, file.read), functions by name, module members by
// module and name (sys.argv, os.path.join), and exception classes by name.
import { readFileSync, writeFileSync } from 'node:fs';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';

const root = join(dirname(fileURLToPath(import.meta.url)), '..', '..');
const library = join(root, 'website', 'src', 'content', 'docs', 'library');
const output = join(root, 'src', 'Taipan.Compiler', 'Semantics', 'BuiltinDocs.cs');

// A page's table rows, as [first cell, second cell], with `\|` in a cell read as `|`.
function rows(page) {
	const found = [];
	for (const line of readFileSync(join(library, page), 'utf8').replace(/\r\n/g, '\n').split('\n')) {
		if (!line.startsWith('| `')) continue;
		const cells = line.trim().slice(1, -1).split(/(?<!\\)\|/).map((c) => c.trim());
		if (cells.length >= 2) found.push([cells[0], cells[1].replaceAll('\\|', '|')]);
	}
	return found;
}

const forms = (cell) => [...cell.matchAll(/`([^`]+)`/g)].map((m) => m[1]);

// Adds a form to its key's list, keeping the pages' order.
const add = (table, key, entry) => table.set(key, [...(table.get(key) ?? []), entry]);

const methods = new Map();
for (const [page, kind] of [['str.md', 'str'], ['list.md', 'list'], ['dict.md', 'dict'], ['set.md', 'set'], ['files.md', 'file']]) {
	for (const [first, doc] of rows(page)) {
		for (const form of forms(first)) {
			const m = form.match(/^(\w+)\.(\w+)\(/);
			if (m) add(methods, `${kind}.${m[2]}`, [form, doc]);
		}
	}
}

const functions = new Map();
for (const page of ['functions.md', 'files.md']) {
	for (const [first, doc] of rows(page)) {
		for (const form of forms(first)) {
			const m = form.match(/^([a-z_]+)\(/);
			if (m) add(functions, m[1], [form, doc]);
		}
	}
}

const members = new Map();
for (const page of ['sys-and-os.md', 'asyncio.md']) {
	for (const [first, doc] of rows(page)) {
		for (const form of forms(first)) {
			const m = form.match(/^(sys|os|asyncio|types)\.((?:path\.)?\w+)/);
			if (m) add(members, `${m[1]}.${m[2]}`, [form, doc]);
		}
	}
}

const exceptions = new Map();
for (const [first, doc] of rows('exceptions.md')) {
	for (const form of forms(first)) {
		const m = form.match(/^(?:asyncio\.)?([A-Z]\w+)$/);
		if (m && !exceptions.has(m[1])) exceptions.set(m[1], doc);
	}
}

const types = [
	['str', "Text: a sequence of characters, which can't be changed."],
	['int', 'A 64-bit whole number; arithmetic that overflows raises `OverflowError`.'],
	['float', 'A 64-bit floating-point number.'],
	['bool', '`True` or `False`.'],
	['list', 'An ordered collection that can change, of one element type.'],
	['dict', 'Keys mapped to values, in the order the keys were added.'],
	['set', 'A collection of distinct values, with no order to rely on.'],
	['tuple', 'A fixed group of values, each with its own type.'],
	['range', 'The whole numbers from a start up to, not including, a stop.'],
	['TextIO', 'An open text file, or one of the standard streams.'],
	['object', 'Any value: the type every other type is assignable to.'],
	['py', 'A Python value, checked by Python when the program runs.'],
];

// A C# string literal.
const cs = (text) => `"${text.replaceAll('\\', '\\\\').replaceAll('"', '\\"')}"`;

// Keys in ordinal order, as C#'s and the tests' comparisons are.
const ordinal = (a, b) => (a < b ? -1 : a > b ? 1 : 0);

function table(name, map, summary) {
	const lines = [
		`    /// <summary>${summary}</summary>`,
		`    public static readonly ImmutableDictionary<string, ImmutableArray<BuiltinDoc>> ${name} = new Dictionary<string, ImmutableArray<BuiltinDoc>>`,
		'    {',
	];
	for (const key of [...map.keys()].sort(ordinal)) {
		const entries = map.get(key).map(([form, doc]) => `new(${cs(form)}, ${cs(doc)})`).join(', ');
		lines.push(`        [${cs(key)}] = [${entries}],`);
	}
	lines.push('    }.ToImmutableDictionary();', '');
	return lines.join('\n');
}

const exceptionLines = [...exceptions.keys()].sort(ordinal).map((k) => `        [${cs(k)}] = ${cs(exceptions.get(k))},`).join('\n');
const typeLines = types.map(([k, v]) => `        [${cs(k)}] = ${cs(v)},`).join('\n');

writeFileSync(output, `using System.Collections.Immutable;
using System.Text.RegularExpressions;

namespace Taipan.Compiler.Semantics;

/// <summary>A call form and what it gives, as the library reference lists it.</summary>
public sealed record BuiltinDoc(string Form, string Doc);

/// <summary>
/// The built-ins' documentation, shown by editors (SPEC §6, §16.3, M58): for each built-in function,
/// method and module member, its call forms and their descriptions, and for each exception class
/// what raises it. Generated from the website's library reference by
/// <c>website/scripts/builtin-docs.mjs</c> (<c>npm run builtin-docs</c>); don't edit it by hand.
/// Methods are keyed <c>str.strip</c>, <c>list.append</c>, <c>dict.get</c>, <c>set.add</c> and
/// <c>file.read</c>; module members <c>sys.argv</c> and <c>os.path.join</c>.
/// </summary>
public static class BuiltinDocs
{
${table('Functions', functions, 'The built-in functions, by name.')}
${table('Methods', methods, "The built-in types' methods, by type and name.")}
${table('Members', members, "The built-in modules' members, by module and name.")}
    /// <summary>What raises each exception class.</summary>
    public static readonly ImmutableDictionary<string, string> Exceptions = new Dictionary<string, string>
    {
${exceptionLines}
    }.ToImmutableDictionary();

    /// <summary>The built-in types, in a sentence each.</summary>
    public static readonly ImmutableDictionary<string, string> Types = new Dictionary<string, string>
    {
${typeLines}
    }.ToImmutableDictionary();

    /// <summary>The links in a description, reduced to their text, since an editor can't follow them.</summary>
    public static string Plain(string doc) => Regex.Replace(doc, @"\\[([^\\]]+)\\]\\([^)]*\\)", "$1");
}
`);
console.log(`wrote ${functions.size} functions, ${methods.size} methods, ${members.size} module members and ${exceptions.size} exception classes`);
