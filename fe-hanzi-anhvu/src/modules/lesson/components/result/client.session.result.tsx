"use client";

import Link from "next/link";
import { useEffect, useMemo, useState } from "react";
import { useParams, useRouter } from "next/navigation";
import { Sparkles } from "lucide-react";
import { Button } from "@/shared/components/ui/button";
import { exerciseApi } from "@/modules/lesson/api/exercise.api";
import { BaseResponse } from "@/shared/types/store.type";
import { CompleteLearningSessionResponse } from "@/modules/lesson/types/topic.type";
import {
    startLearningTopicQueryKey,
    resultCompleteSessionQueryKey,
    useResultCompleteSession,
    useStartLearningTopicMutation,
} from "@/modules/lesson/hooks/use.topic.tanstack";
import { useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

const formatDuration = (seconds: number) => {
    if (!Number.isFinite(seconds) || seconds < 0) {
        return "—";
    }
    const minutes = Math.floor(seconds / 60);
    const remaining = seconds % 60;
    return `${minutes} phút ${remaining} giây`;
};

const formatScore = (value: number) => {
    if (!Number.isFinite(value)) {
        return "—";
    }
    return `${Math.round(value)}%`;
};

const ResultClientComponent = () => {
    const params = useParams();
    const router = useRouter();
    const queryClient = useQueryClient();
    const slug = String(params["slug"] ?? "");
    const topicSlug = String(params["slug-topic"] ?? "");
    const sessionIdFromRoute = String(params["session-id"] ?? "");
    const startLearningMutation = useStartLearningTopicMutation();
    const [isInit, setIsInit] = useState(false);
    const [status, setStatus] = useState<"loading" | "error" | "success">(
        "loading",
    );
    const [message, setMessage] = useState<string>("");
    const [result, setResult] =
        useState<CompleteLearningSessionResponse | null>(null);
    const resolveSessionId = useMemo(() => {
        return sessionIdFromRoute;
    }, [sessionIdFromRoute]);
    const resultCompleteSessionQuery =
        useResultCompleteSession(resolveSessionId);
    const cachedResultCompleteSession = queryClient.getQueryData<
        BaseResponse<CompleteLearningSessionResponse>
    >(resultCompleteSessionQueryKey(sessionIdFromRoute));

    // load result from cache if exist to prevent call api again when just come from exercise page with complete session action
    useEffect(() => {
        if (isInit) {
            return;
        }

        if (
            cachedResultCompleteSession?.success &&
            cachedResultCompleteSession.data
        ) {
            setResult(cachedResultCompleteSession.data);
            setStatus("success");
            setIsInit(true);
        }
    }, [cachedResultCompleteSession]);

    // Nếu không có cache, trigger query API
    useEffect(() => {
        if (isInit) {
            return; // Đã có data từ cache, bỏ qua
        }

        if (!resolveSessionId || !topicSlug) {
            setStatus("error");
            setMessage("Không tìm thấy phiên học để hiển thị kết quả.");
            return;
        }

        const dataResult = resultCompleteSessionQuery.data;
        if (!dataResult) {
            if (resultCompleteSessionQuery.isError) {
                setStatus("error");
                setMessage(
                    "Không thể tải kết quả phiên học. Vui lòng thử lại.",
                );
            }
            return;
        }

        if (dataResult.success && dataResult.data) {
            setResult(dataResult.data);
            setStatus("success");
        }
    }, [
        resolveSessionId,
        topicSlug,
        isInit,
        resultCompleteSessionQuery.data,
        resultCompleteSessionQuery.isError,
    ]);

    const summaryItems = useMemo(() => {
        if (!result) {
            return [] as { label: string; value: string }[];
        }

        return [
            { label: "Tổng điểm", value: formatScore(result.totalScore) },
            { label: "Đúng", value: String(result.totalCorrect) },
            { label: "Sai", value: String(result.totalWrong) },
            { label: "Nghe đúng", value: String(result.scoreListening) },
            { label: "Đọc đúng", value: String(result.scoreReading) },
            {
                label: "Thời gian làm",
                value: formatDuration(result.timeSpentSeconds),
            },
            { label: "Tổng số bài tập", value: String(result.totalExercises) },
        ];
    }, [result]);

    if (status === "loading") {
        return (
            <div className="flex min-h-48 items-center justify-center rounded-2xl border border-slate-200 bg-white p-4 shadow-sm">
                <div className="flex flex-col items-center gap-2.5 text-slate-600">
                    <Sparkles className="h-7 w-7 animate-pulse text-emerald-500" />
                    <p className="text-sm font-medium">Đang tải kết quả...</p>
                </div>
            </div>
        );
    }

    if (status === "error") {
        return (
            <div className="rounded-2xl border border-red-200 bg-red-50 p-4 text-sm text-red-700 shadow-sm">
                <p>{message}</p>
                <div className="mt-4">
                    <Button asChild variant="outline">
                        <Link href={`/u/${slug}`}>Quay lại khóa học</Link>
                    </Button>
                </div>
            </div>
        );
    }

    return (
        <div className="space-y-4 rounded-2xl border border-slate-200 bg-white p-5 shadow-sm">
            <div className="space-y-2">
                <h1 className="text-xl font-semibold text-slate-900">
                    Kết quả phiên học
                </h1>
                <p className="text-sm text-slate-500">
                    Chủ đề: <span className="font-medium">{topicSlug}</span>
                </p>
                {result?.completedAt ? (
                    <p className="text-xs text-slate-400">
                        Hoàn thành lúc:{" "}
                        {new Date(result.completedAt).toLocaleString("vi-VN")}
                    </p>
                ) : null}
            </div>

            <div className="grid grid-cols-1 gap-3 sm:grid-cols-2 xl:grid-cols-3">
                {summaryItems.map((item) => (
                    <div
                        key={item.label}
                        className="rounded-xl border border-slate-200 bg-slate-50 px-3 py-2"
                    >
                        <p className="text-xs font-semibold uppercase tracking-wide text-slate-500">
                            {item.label}
                        </p>
                        <p className="mt-1 text-lg font-semibold text-slate-900">
                            {item.value}
                        </p>
                    </div>
                ))}
            </div>

            <div className="flex flex-wrap gap-2">
                <Button asChild variant="outline">
                    <Link href={`/u/${slug}`}>Về danh sách chủ đề</Link>
                </Button>
                <Button
                    onClick={async () => {
                        if (!topicSlug) {
                            return;
                        }
                        const restart =
                            await startLearningMutation.mutateAsync(topicSlug);
                        if (!restart.success || !restart.data) {
                            setStatus("error");
                            setMessage(
                                restart.message || "Không thể bắt đầu học lại.",
                            );
                            return;
                        }
                        queryClient.setQueryData(
                            startLearningTopicQueryKey(topicSlug),
                            restart,
                        );
                        router.push(
                            `/u/${slug}/${topicSlug}/${restart.data.sessionId}`,
                        );
                    }}
                >
                    Học lại
                </Button>
            </div>
        </div>
    );
};

export default ResultClientComponent;
