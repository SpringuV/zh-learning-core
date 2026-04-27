"use client";
import {
    useGetTopicsForClient,
    startLearningTopicQueryKey,
    useCountinueLearningTopic,
    useStartLearningTopicMutation,
} from "@/modules/lesson/hooks/use.topic.tanstack";
import { Button } from "@/shared/components/ui/button";
import { Sparkles } from "lucide-react";
import { useQueryClient } from "@tanstack/react-query";
import { useSession } from "next-auth/react";
import { useParams, useRouter } from "next/navigation";
import { TopicClientDashboardItemResponse } from "@/modules/lesson/types/topic.type";
import { AxiosError } from "axios";
import { useState } from "react";
import { topicApi } from "@/modules/lesson/api/topic.api";

const formatMinutes = (minutes: number) => {
    return `${minutes} phút`;
};

const getTopicStatusText = (status: string) => {
    if (status === "NotStarted") return "Chưa bắt đầu";
    if (status === "InProgress") return "Đang học";
    if (status === "Completed") return "Đã hoàn thành";
    if (status === "Abandoned") return "Đã tạm dừng";
    return "Chưa bắt đầu";
};

const getTopicActionText = (status: string) => {
    if (status === "InProgress") return "Học tiếp";
    if (status === "Completed") return "Học lại";
    if (status === "NotStarted") return "Bắt đầu học";
    return "Bắt đầu học";
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
    const router = useRouter();
    const { slug: slug } = useParams();
    const topicClientDashboardTanstack = useGetTopicsForClient(String(slug));
    // const [slugQuery, setSlugQuery] = useState<string | null>(null);
    const response = topicClientDashboardTanstack.data?.data;
    const topics = response?.data ?? [];
    const { data: session } = useSession();
    const startLearningMutation = useStartLearningTopicMutation();
    const queryClient = useQueryClient();
    // nếu chưa có slugQuery thì sẽ không chạy query tiếp tục học, khi người dùng click vào bắt đầu học
    // hoặc tiếp tục học thì sẽ set slugQuery và query sẽ tự động chạy để lấy dữ liệu session mới nhất cho chủ đề đó,
    // sau đó điều hướng người dùng đến trang học tập với dữ liệu đã được cập nhật trong cache,
    // tránh việc điều hướng rồi mới chạy query để lấy dữ liệu session,
    // như vậy sẽ tối ưu được trải nghiệm người dùng và đảm bảo luôn có dữ liệu mới nhất
    // khi bắt đầu hoặc tiếp tục học
    // const countinueLearningQuery = useCountinueLearningTopic(slugQuery ?? ""); // Reuse the same mutation for continue learning since the API response is the same as start learning
    // #region handle session learning
    const handleStartOrCountinueLearning = async (
        topic: TopicClientDashboardItemResponse,
    ) => {
        const userId = session?.user?.id;

        if (!userId) {
            alert("Vui lòng đăng nhập để bắt đầu học.");
            return;
        }
        if (!topic.slug) {
            alert("Không tìm thấy chủ đề học.");
            return;
        }
        switch (topic.status) {
            case "InProgress":
                try {
                    // setSlugQuery(topic.slug); // Cập nhật slug cho query tiếp tục học
                    // if (response?.errorCode) {
                    //     alert(response.message || "Không thể tiếp tục học.");
                    //     return;
                    // }
                    const result = await queryClient.fetchQuery({
                        queryKey: ["continue-learning-topic", topic.slug],
                        queryFn: () =>
                            topicApi
                                .continueLearningSessionForTopicClient(
                                    topic.slug,
                                )
                                .then((res) => res.data),
                    });

                    queryClient.setQueryData(
                        startLearningTopicQueryKey(topic.slug),
                        result,
                    );
                    router.push(
                        `/u/${String(slug)}/${topic.slug}/${result.data?.sessionId}`,
                    );
                } catch (error) {
                    if (error instanceof AxiosError) {
                        console.error("Continue learning error:", error);
                        alert(
                            `Đã xảy ra lỗi khi tiếp tục học. Vui lòng thử lại sau. Chi tiết lỗi: ${error.response?.data?.message}`,
                        );
                        return;
                    }
                    throw error;
                }
                break;
            case "NotStarted":
                try {
                    const result = await startLearningMutation.mutateAsync(
                        String(topic.slug),
                    );
                    console.log("Start learning result:", result);
                    if (!result.success || !result.data) {
                        alert(result.message || "Không thể bắt đầu học.");
                        return;
                    }
                    // Cập nhật cache với dữ liệu mới nhất từ API sau khi bắt đầu hoặc tiếp tục học
                    if (result?.success) {
                        queryClient.setQueryData(
                            startLearningTopicQueryKey(topic.slug),
                            result,
                        );
                        router.push(
                            `/u/${String(slug)}/${topic.slug}/${result.data?.sessionId}`,
                        );
                    }
                } catch (error) {
                    if (error instanceof AxiosError) {
                        console.error("Start learning error:", error);
                        alert(
                            `Đã xảy ra lỗi khi bắt đầu học. Vui lòng thử lại sau. Chi tiết lỗi: ${error.response?.data?.message}`,
                        );
                    }
                }
                break;
        }
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
                            <InfoRow
                                label="Trạng thái"
                                value={getTopicStatusText(topic.status)}
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
                            onClick={() =>
                                void handleStartOrCountinueLearning(topic)
                            }
                            size="sm"
                            variant="outline"
                            disabled={startLearningMutation.isPending}
                            className="mt-3 bg-primary text-white! w-full border-slate-200 hover:bg-primary/80 disabled:cursor-not-allowed disabled:opacity-70"
                        >
                            {startLearningMutation.isPending
                                ? "Đang khởi tạo..."
                                : getTopicActionText(topic.status)}
                        </Button>
                    </article>
                ))}
            </section>
        </div>
    );
};

export default TopicDashboardClientComponent;
