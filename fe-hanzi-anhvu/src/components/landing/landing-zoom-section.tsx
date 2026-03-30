import Link from "next/link";
import { CalendarClock, MessageCircle } from "lucide-react";
import { Button } from "@/components/ui/button";
import {
    Card,
    CardContent,
    CardDescription,
    CardFooter,
    CardHeader,
    CardTitle,
} from "@/components/ui/card";
import { zoomClasses } from "./landing-data";

export function LandingZoomSection() {
    return (
        <section
            id="zoom"
            className="mx-auto max-w-6xl px-4 pb-16 sm:px-6 lg:px-8"
        >
            <Card className="border-[#cad5dc] bg-linear-to-r from-white to-[#eef5f8]">
                <CardHeader>
                    <CardTitle className="flex items-center gap-2 text-2xl">
                        <CalendarClock className="size-5 text-primary" />
                        Lớp Zoom Trực Tiếp Tuần Này
                    </CardTitle>
                    <CardDescription>
                        Vào phòng học trực tiếp để được sửa phát âm và phân tích
                        lời nói.
                    </CardDescription>
                </CardHeader>
                <CardContent className="grid gap-4 md:grid-cols-2">
                    {zoomClasses.map((item) => (
                        <div
                            key={item.title}
                            className="rounded-xl border border-[#d6dee3] bg-white p-4"
                        >
                            <p className="text-lg font-bold">{item.title}</p>
                            <p className="mt-1 text-sm text-muted-foreground">
                                {item.time}
                            </p>
                            <div className="mt-3 flex items-center justify-between">
                                <span className="rounded-full bg-primary/10 px-3 py-1 text-xs font-medium text-primary">
                                    {item.seats}
                                </span>
                                <Button size="sm">Tham gia</Button>
                            </div>
                        </div>
                    ))}
                </CardContent>
                <CardFooter
                    id="roadmap"
                    className="flex flex-col items-start gap-3 border-t border-[#dde5ea] pt-5 sm:flex-row sm:items-center sm:justify-between"
                >
                    <div>
                        <p className="text-sm text-muted-foreground">
                            Roadmap ca nhan
                        </p>
                        <p className="text-base font-semibold">
                            Hoan thanh 5 buoi review va 1 lop Zoom speaking
                            truoc Chu nhat.
                        </p>
                    </div>
                    <Button asChild>
                        <Link href="/dashboard">
                            <MessageCircle className="size-4" />
                            Mo dashboard hoc tap
                        </Link>
                    </Button>
                </CardFooter>
            </Card>
        </section>
    );
}
