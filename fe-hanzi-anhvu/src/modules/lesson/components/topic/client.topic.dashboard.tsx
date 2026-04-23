"use client";
import { useGetTopicsForClient } from "@/modules/lesson/hooks/use.topic.tanstack";
import { Button } from "@/shared/components/ui/button";
import { Sparkles } from "lucide-react";
import { useSession } from "next-auth/react";
import { useParams } from "next/navigation";

const formatMinutes = (minutes: number) => {
    return `${minutes} phút`;
};

type InfoRowProps = {
    label: string;
    value: string;
};

function InfoRow({ label, value }: InfoRowProps) {
    return (
        <div className="xl:flex xl:gap-2 xl:items-center xl:justify-between rounded-xl border border-slate-200 bg-slate-50 px-2.5 py-1.5">
            <p className="text-xs font-semibold uppercase tracking-wide text-slate-500">
                {label}
            </p>
            <p className="mt-1 xl:mt-0 wrap-break-word text-sm font-medium leading-5 text-slate-900">
                {value}
            </p>
        </div>
    );
}

const TopicDashboardClientComponent = () => {
    const { slug: slug } = useParams();
    const topicClientDashboardTanstack = useGetTopicsForClient(String(slug));
    const response = topicClientDashboardTanstack.data?.data;
    const topics = response?.data ?? [];
    const { data: session } = useSession();

    const handleStartLearning = (topicId: string) => {
        if (!session) {
            alert("Vui lòng đăng nhập để bắt đầu học.");
            return;
        }
        console.log("User ID:", session.user?.id);
        console.log("Start learning topic with ID:", topicId);
    };

    if (!slug) {
        return (
            <div className="rounded-xl border border-dashed border-slate-300 bg-white p-4 text-sm text-slate-500 shadow-sm">
                Không tìm thấy khóa học.
            </div>
        );
    }

    if (
        topicClientDashboardTanstack.isLoading ||
        topicClientDashboardTanstack.isFetching
    ) {
        return (
            <div className="flex min-h-48 items-center justify-center rounded-2xl border border-slate-200 bg-white p-4 shadow-sm">
                <div className="flex flex-col items-center gap-2.5 text-slate-600">
                    <Sparkles className="h-7 w-7 animate-pulse text-emerald-500" />
                    <p className="text-sm font-medium">
                        Đang tải danh sách chủ đề...
                    </p>
                </div>
            </div>
        );
    }

    if (topicClientDashboardTanstack.isError || response?.success === false) {
        return (
            <div className="rounded-xl border border-red-200 bg-red-50 p-4 text-sm text-red-600 shadow-sm">
                {response?.message ??
                    "Không thể tải danh sách chủ đề cho khóa học này."}
            </div>
        );
    }

    if (topics.length === 0) {
        return (
            <div className="rounded-2xl border border-slate-200 bg-white p-4 shadow-sm">
                <div className="flex flex-col items-center justify-center gap-2.5 py-8 text-center">
                    <Sparkles className="h-8 w-8 text-slate-300" />
                    <div>
                        <h2 className="text-base font-semibold text-slate-900">
                            Chưa có topic nào trong khóa học này
                        </h2>
                        <p className="mt-1 text-sm text-slate-500">
                            Khi dữ liệu topic được trả về từ API, chúng sẽ xuất
                            hiện ở đây.
                        </p>
                    </div>
                </div>
            </div>
        );
    }

    return (
        <div className="space-y-4">
            <section className="grid grid-cols-1 gap-3 md:grid-cols-2 2xl:grid-cols-3">
                {topics.map((topic) => (
                    <article
                        key={topic.id}
                        className="rounded-2xl border border-slate-200 bg-white p-4 shadow-sm transition-shadow hover:shadow-md"
                    >
                        <div className="flex flex-wrap items-start justify-between gap-2.5">
                            <div className="min-w-0 space-y-2">
                                <div>
                                    <h2 className="text-lg font-semibold leading-6 text-slate-900">
                                        {topic.title}
                                    </h2>
                                    <p className="mt-1 line-clamp-3 text-sm leading-5 text-slate-500">
                                        {topic.description}
                                    </p>
                                </div>
                            </div>
                        </div>

                        <div className="mt-4 grid grid-cols-2 gap-2">
                            <InfoRow
                                label="Thời gian ước tính"
                                value={formatMinutes(
                                    topic.estimatedTimeMinutes,
                                )}
                            />
                            <InfoRow
                                label="Tổng bài tập"
                                value={`${topic.totalExercises}`}
                            />
                            {topic.topicType === "Exam" ? (
                                <>
                                    <InfoRow
                                        label="Năm thi"
                                        value={
                                            topic.examYear != null
                                                ? `${topic.examYear}`
                                                : "—"
                                        }
                                    />
                                    <InfoRow
                                        label="Mã đề"
                                        value={
                                            topic.examCode?.trim()
                                                ? topic.examCode
                                                : "—"
                                        }
                                    />
                                </>
                            ) : null}
                        </div>
                        <Button
                            type="button"
                            onClick={() => handleStartLearning(topic.id)}
                            size="sm"
                            variant="outline"
                            className="mt-3 bg-primary text-white! w-full border-slate-200 hover:bg-primary/80"
                        >
                            Bắt đầu học
                        </Button>
                    </article>
                ))}
            </section>
        </div>
    );
};

export default TopicDashboardClientComponent;
