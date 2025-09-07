#!/usr/bin/env python3
import argparse
import os
import sys
import tempfile
from pathlib import Path

DEFAULT_SKIP_DIRS = {".git", ".svn", ".hg", "__pycache__", "node_modules", "bin", "obj", ".idea", ".vs"}
DEFAULT_BINARY_EXTS = {
    ".png", ".jpg", ".jpeg", ".gif", ".webp", ".svg", ".ico",
    ".mp3", ".wav", ".flac", ".ogg",
    ".mp4", ".mkv", ".mov", ".avi", ".webm",
    ".zip", ".7z", ".rar", ".gz", ".tar", ".bz2", ".xz",
    ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx",
    ".exe", ".dll", ".so", ".dylib",
    ".ttf", ".otf", ".woff", ".woff2",
    ".psd", ".ai", ".sketch", ".fig"
}
DEFAULT_ENCODINGS = ["utf-8", "utf-8-sig", "cp1254", "latin-1"]

def parse_ext_list(value: str):
    if not value:
        return set()
    items = []
    for p in value.split(","):
        p = p.strip()
        if not p:
            continue
        if not p.startswith("."):
            p = "." + p
        items.append(p.lower())
    return set(items)

def read_text_with_encodings(p: Path, encodings):
    for enc in encodings:
        try:
            with p.open("r", encoding=enc, errors="strict") as f:
                return f.read(), enc
        except Exception:
            continue
    return None, None

def write_text_atomic(p: Path, content: str, encoding: str):
    parent = p.parent
    with tempfile.NamedTemporaryFile("w", delete=False, dir=parent, encoding=encoding) as tmp:
        tmp.write(content)
        tmp_path = Path(tmp.name)
    os.replace(tmp_path, p)

def replace_in_file(p: Path, needle: str, new: str, encodings, dry_run: bool):
    text, enc = read_text_with_encodings(p, encodings)
    if text is None:
        return 0, False
    count = text.count(needle)
    if count == 0:
        return 0, False
    replaced = text.replace(needle, new)
    if not dry_run:
        write_text_atomic(p, replaced, enc or "utf-8")
    return count, True

def rename_file_if_needed(p: Path, needle: str, new: str, dry_run: bool):
    name = p.name
    if needle not in name:
        return p, 0, False
    new_name = name.replace(needle, new)
    target = p.with_name(new_name)
    if target.exists():
        base = target.stem
        suf = target.suffix
        i = 1
        while True:
            candidate = target.with_name(f"{base}__RENAMED_{i}{suf}")
            if not candidate.exists():
                target = candidate
                break
            i += 1
    if not dry_run:
        p.rename(target)
    return target, 1, True

def rename_dir_if_needed(p: Path, needle: str, new: str, dry_run: bool):
    name = p.name
    if needle not in name:
        return p, 0, False
    new_name = name.replace(needle, new)
    target = p.with_name(new_name)
    if target.exists():
        base = new_name
        i = 1
        while True:
            candidate = p.with_name(f"{base}__RENAMED_{i}")
            if not candidate.exists():
                target = candidate
                break
            i += 1
    if not dry_run:
        p.rename(target)
    return target, 1, True

def should_skip_file(p: Path, include_exts, exclude_exts, force_binary: bool):
    ext = p.suffix.lower()
    if include_exts and ext not in include_exts:
        return True
    if not force_binary:
        if ext in DEFAULT_BINARY_EXTS or ext in exclude_exts:
            return True
    return False

def main():
    ap = argparse.ArgumentParser(prog="replace_text", description="Klasör ve dosya adlarında ve dosya içeriklerinde birebir eşleşen metni değiştirir (case-sensitive).")
    ap.add_argument("root", help="İşlenecek klasör yolu")
    ap.add_argument("from_text", help="Aranacak cümle/ifade (case-sensitive)")
    ap.add_argument("to_text", help="Yerine yazılacak cümle/ifade")
    ap.add_argument("--include-ext", default="", help="Sadece bu uzantıları işle (.cs,.cshtml,.ts gibi, virgülle ayır)")
    ap.add_argument("--exclude-ext", default="", help="Bu uzantıları hariç tut (.log,.json gibi, virgülle ayır)")
    ap.add_argument("--follow-symlinks", action="store_true", help="Sembolik linkleri takip et")
    ap.add_argument("--force-binary", action="store_true", help="Binary uzantı filtresini devre dışı bırak")
    ap.add_argument("--encodings", default=",".join(DEFAULT_ENCODINGS), help="Denenecek encoding listesi, sıra önemlidir")
    ap.add_argument("--dry-run", action="store_true", help="Sadece rapor üret, yazma/yeniden adlandırma yapma")
    args = ap.parse_args()

    root = Path(args.root).resolve()
    if not root.exists() or not root.is_dir():
        print("Hata: Klasör bulunamadı veya dizin değil:", root, file=sys.stderr)
        sys.exit(2)

    include_exts = parse_ext_list(args.include_ext)
    exclude_exts = parse_ext_list(args.exclude_ext)
    encodings = [e.strip() for e in args.encodings.split(",") if e.strip()]
    if not encodings:
        encodings = DEFAULT_ENCODINGS

    total_files_scanned = 0
    total_files_changed = 0
    total_content_replacements = 0
    total_files_renamed = 0
    total_dirs_renamed = 0

    for dirpath, dirnames, filenames in os.walk(root, followlinks=args.follow_symlinks):
        dp = Path(dirpath)
        filtered = []
        for d in dirnames:
            if d in DEFAULT_SKIP_DIRS:
                continue
            old_dir = dp / d
            new_dir, hit, renamed = rename_dir_if_needed(old_dir, args.from_text, args.to_text, args.dry_run)
            if renamed:
                total_dirs_renamed += 1
                print(f"[OK] {old_dir} -> RENAMED_DIR")
            filtered.append(new_dir.name if renamed else d)
        dirnames[:] = filtered
        for fname in filenames:
            p = dp / fname
            if not p.is_file():
                continue
            if should_skip_file(p, include_exts, exclude_exts, args.force_binary):
                continue
            total_files_scanned += 1
            ren_target, rename_hits, renamed = rename_file_if_needed(p, args.from_text, args.to_text, args.dry_run)
            if renamed:
                total_files_renamed += 1
                p = ren_target
            cnt, changed = replace_in_file(p, args.from_text, args.to_text, encodings, args.dry_run)
            if changed:
                total_files_changed += 1
                total_content_replacements += cnt
            action = []
            if renamed:
                action.append("RENAMED_FILE")
            if changed:
                action.append(f"CONTENT x{cnt}")
            if action:
                print(f"[OK] {p} -> {' & '.join(action)}")
    print("\nÖzet")
    print("Tarama Yapılan Dosya:", total_files_scanned)
    print("İçeriği Değişen Dosya:", total_files_changed)
    print("Toplam İçerik Değişimi:", total_content_replacements)
    print("Yeniden Adlandırılan Dosya:", total_files_renamed)
    print("Yeniden Adlandırılan Klasör:", total_dirs_renamed)
    if args.dry_run:
        print("\nNot: DRY-RUN modundaydı; hiçbir kalıcı değişiklik yapılmadı.")

if __name__ == "__main__":
    main()
