import Link from "next/link";
import { Send, SunMedium } from "lucide-react";
import { Button } from "@/components/ui/button";

export function LandingHeader() {
    return (
        <header className="flex items-center justify-between rounded-2xl border border-white/20 bg-white/88 px-4 py-3 text-[#1f2a37] shadow-lg backdrop-blur md:px-6">
            <Link href="/" className="flex items-center gap-2">
                <div className="flex h-10 w-10 items-center justify-center rounded-full bg-primary text-white">
                    汉
                </div>
                <div>
                    <p className="text-lg font-bold">HanZi Live</p>
                    <p className="text-xs text-slate-500">
                        Hoc tieng Trung thuc te
                    </p>
                </div>
            </Link>

            <nav className="hidden items-center gap-7 text-sm font-semibold lg:flex">
                <Link href="#features" className="hover:text-primary">
                    Khoa Hoc
                </Link>
                <Link href="#review" className="hover:text-primary">
                    Review
                </Link>
                <Link href="#zoom" className="hover:text-primary">
                    Zoom Truc Tiep
                </Link>
                <Link href="#roadmap" className="hover:text-primary">
                    Lo Trinh
                </Link>
            </nav>

            <div className="flex items-center gap-2">
                <Button
                    asChild
                    variant="outline"
                    size="sm"
                    className="hidden border-slate-300 bg-white/80 text-slate-700 hover:bg-white sm:inline-flex"
                >
                    <Link href="/auth/login">Dang nhap</Link>
                </Button>
                <Button
                    asChild
                    size="sm"
                    className="hidden bg-primary text-white hover:bg-primary/90 sm:inline-flex"
                >
                    <Link href="/auth/register">Dang ky</Link>
                </Button>
                <Button
                    variant="ghost"
                    size="icon-sm"
                    className="text-[#0369a1]"
                >
                    <Send className="size-4" />
                </Button>
                <Button
                    variant="ghost"
                    size="icon-sm"
                    className="text-[#ea580c]"
                >
                    <SunMedium className="size-4" />
                </Button>
            </div>
        </header>
    );
}
