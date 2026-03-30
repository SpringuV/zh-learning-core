import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";

export function LandingReviewStats() {
    return (
        <section
            id="review"
            className="mx-auto mt-14 max-w-6xl px-4 pb-8 sm:px-6 lg:px-8"
        >
            <div className="grid gap-5 md:grid-cols-3">
                <Card className="border-[#d5d8dd] bg-white/90">
                    <CardHeader>
                        <CardTitle className="text-lg">Streak On Tap</CardTitle>
                    </CardHeader>
                    <CardContent>
                        <p className="text-4xl font-black text-primary">
                            21 ngay
                        </p>
                        <p className="mt-2 text-sm text-muted-foreground">
                            Duy tri deu dan de giu phan xa va nho tu ben.
                        </p>
                    </CardContent>
                </Card>
                <Card className="border-[#d5d8dd] bg-white/90">
                    <CardHeader>
                        <CardTitle className="text-lg">
                            Bai Review Hom Nay
                        </CardTitle>
                    </CardHeader>
                    <CardContent>
                        <p className="text-4xl font-black text-primary">
                            5 bai
                        </p>
                        <p className="mt-2 text-sm text-muted-foreground">
                            Gom 2 bai nghe, 2 bai noi, 1 bai tu vung HSK.
                        </p>
                    </CardContent>
                </Card>
                <Card className="border-[#d5d8dd] bg-white/90">
                    <CardHeader>
                        <CardTitle className="text-lg">Muc Tieu Tuan</CardTitle>
                    </CardHeader>
                    <CardContent>
                        <p className="text-4xl font-black text-primary">80%</p>
                        <p className="mt-2 text-sm text-muted-foreground">
                            Hoan thanh roadmap de mo khoa lop speaking nang cao.
                        </p>
                    </CardContent>
                </Card>
            </div>
        </section>
    );
}
