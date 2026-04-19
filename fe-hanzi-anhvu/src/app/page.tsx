import { Metadata } from "next";
import { LandingFeatures } from "@/shared/components/landing/landing-features";
import { LandingHeader } from "@/shared/components/landing/landing-header";
import { LandingHero } from "@/shared/components/landing/landing-hero";
import { LandingZoomSection } from "@/shared/components/landing/landing-zoom-section";

export const metadata: Metadata = {
    title: "Landing Page - HanZi Live",
    description:
        "Landing page học tiếng Trung với ôn tập, ôn thi HSKK và lớp học trực tiếp",
};

export default function Home() {
    return (
        <main className="min-h-screen bg-[#f5f5f2] text-[#13212b]">
            <section className="relative overflow-hidden bg-[#0d2833] pb-30 text-white">
                <div
                    className="absolute inset-0 bg-cover bg-center opacity-35"
                    style={{
                        backgroundImage:
                            "url('https://images.unsplash.com/photo-1491438590914-bc09fcaaf77a?auto=format&fit=crop&w=1800&q=80')",
                    }}
                />
                <div className="absolute inset-0 bg-linear-to-r from-[#08202d]/90 via-[#0d2833]/75 to-[#112e3a]/70" />

                <div className="relative mx-auto max-w-6xl px-4 pt-4 sm:px-6 lg:px-8">
                    <LandingHeader />
                    <LandingHero />
                </div>
            </section>
            <LandingFeatures />
            <LandingZoomSection />
        </main>
    );
}
