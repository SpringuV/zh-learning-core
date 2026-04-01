import Link from "next/link";
import { BookOpen, Flame, Video } from "lucide-react";
import { Button } from "@/shared/components/ui/button";

export function LandingHero() {
    return (
        <div className="max-w-3xl py-16 sm:py-24">
            <p className="inline-flex items-center gap-2 rounded-full border border-white/35 bg-white/10 px-3 py-1 text-xs uppercase tracking-widest text-white/90">
                <Flame className="size-3.5" />
                Hi there, welcome to HanZiAnhVu!
            </p>
            <h1 className="mt-5 text-4xl font-black leading-[1.08] uppercase sm:text-6xl">
                Beyond Your Chinese
                <br />
                Speaking Skills
            </h1>
            <p className="mt-5 max-w-2xl text-base text-white/85 sm:text-lg">
                Day la nen tang cho nguoi hoc muon tien bo thuc te: on tap moi
                ngay, bam muc tieu ro rang, va tham gia lop Zoom truc tiep de
                sua phan xa giao tiep.
            </p>
            <div className="mt-8 flex flex-wrap gap-3">
                <Button
                    asChild
                    size="lg"
                    className="bg-[#14b8a6] text-white hover:bg-[#0f9e90]"
                >
                    <Link href="#review">
                        <BookOpen className="size-4" />
                        Bat dau on tap
                    </Link>
                </Button>
                <Button
                    asChild
                    size="lg"
                    variant="outline"
                    className="border-white/45 bg-white/10 text-white hover:bg-white/20"
                >
                    <Link href="#zoom">
                        <Video className="size-4" />
                        Xem lich Zoom
                    </Link>
                </Button>
            </div>
        </div>
    );
}
