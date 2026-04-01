import {
    Card,
    CardContent,
    CardDescription,
    CardHeader,
    CardTitle,
} from "@/shared/components/ui/card";
import { reviewFeatures } from "./landing-data";

export function LandingFeatures() {
    return (
        <section
            id="features"
            className="relative -mt-20 mx-auto max-w-6xl px-4 sm:px-6 lg:px-8"
        >
            <div className="grid gap-4 md:grid-cols-3">
                {reviewFeatures.map((item) => {
                    const Icon = item.icon;
                    return (
                        <Card
                            key={item.title}
                            className="border-0 bg-linear-to-br from-[#b91c1c] to-[#991b1b] text-white shadow-2xl"
                        >
                            <CardHeader className="pb-2">
                                <div className="mb-4 inline-flex h-12 w-12 items-center justify-center rounded-full border border-amber-200/45 bg-amber-400/15">
                                    <Icon className="size-5 text-amber-200" />
                                </div>
                                <CardTitle className="text-2xl font-bold">
                                    {item.title}
                                </CardTitle>
                            </CardHeader>
                            <CardContent>
                                <CardDescription className="text-sm leading-relaxed text-red-100">
                                    {item.description}
                                </CardDescription>
                            </CardContent>
                        </Card>
                    );
                })}
            </div>
        </section>
    );
}
