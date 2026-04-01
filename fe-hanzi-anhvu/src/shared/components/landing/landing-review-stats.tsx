import {
    Card,
    CardContent,
    CardHeader,
    CardTitle,
} from "@/shared/components/ui/card";

export function LandingReviewStats() {
    return (
        <section
            id="review"
            className="mx-auto mt-14 max-w-6xl px-4 pb-8 sm:px-6 lg:px-8"
        >
            <div className="grid gap-5 md:grid-cols-3">
                <Card className="border-[#d5d8dd] bg-white/90">
                    <CardHeader>
                        <CardTitle className="text-lg">Streak Ôn Tập</CardTitle>
                    </CardHeader>
                    <CardContent>
                        <p className="text-4xl font-black text-primary">
                            21 ngày
                        </p>
                        <p className="mt-2 text-sm text-muted-foreground">
                            Duy trì đều đặn để giữ vững kiến thức và nhớ từ
                            vựng.
                        </p>
                    </CardContent>
                </Card>
                <Card className="border-[#d5d8dd] bg-white/90">
                    <CardHeader>
                        <CardTitle className="text-lg">
                            Bài Review Hôm Nay
                        </CardTitle>
                    </CardHeader>
                    <CardContent>
                        <p className="text-4xl font-black text-primary">
                            5 bài
                        </p>
                        <p className="mt-2 text-sm text-muted-foreground">
                            Gom 2 bài nghe, 2 bài nói, 1 bài từ vựng HSK.
                        </p>
                    </CardContent>
                </Card>
                <Card className="border-[#d5d8dd] bg-white/90">
                    <CardHeader>
                        <CardTitle className="text-lg">Mục Tiêu Tuần</CardTitle>
                    </CardHeader>
                    <CardContent>
                        <p className="text-4xl font-black text-primary">80%</p>
                        <p className="mt-2 text-sm text-muted-foreground">
                            Hoàn thành roadmap để mở khóa lớp speaking nâng cao.
                        </p>
                    </CardContent>
                </Card>
            </div>
        </section>
    );
}
