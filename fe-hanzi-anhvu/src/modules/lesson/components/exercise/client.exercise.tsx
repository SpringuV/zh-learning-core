"use client";

import { startLearningTopicQueryKey } from "@/modules/lesson/hooks/use.topic.tanstack";
import {
    CountinueLearningSessionResponse,
    LearningExerciseSessionItemDTOResponse,
    StartLearningTopicResponse,
} from "@/modules/lesson/types/topic.type";
import NavigationActions from "@/modules/lesson/components/exercise/components/NavigationActions";
import QuestionList from "@/modules/lesson/components/exercise/components/QuestionList";
import SessionHeader from "@/modules/lesson/components/exercise/components/SessionHeader";
import { BaseResponse } from "@/shared/types/store.type";
import { useQueryClient } from "@tanstack/react-query";
import { useParams } from "next/navigation";
import { useSession } from "next-auth/react";
import { useCallback, useEffect, useMemo, useState } from "react";
import { ExerciseSessionPracticeItemWithoutAnswerResponse } from "@/modules/lesson/types/exercise.type";
import {
    useExerciseSessionPracticeWithoutAnswer,
    useGetSessionItemsSnapshot,
    useSaveAnswer,
} from "@/modules/lesson/hooks/use.exercise.tanstack";
import { Button } from "@/shared/components/ui/button";

NavigationActions.displayName = "NavigationActions";

const normalizeRouteParam = (param: string | string[] | undefined): string => {
    if (Array.isArray(param)) {
        return param[0] || "";
    }
    return param || "";
};
const optionLabels = "abcdefghijklmnopqrstuvwxyz".toUpperCase();

const ExerciseOptionButton = ({
    option,
    label,
    selected,
    disabled,
    onSelect,
}: {
    option: { id: string; text: string };
    label: string;
    selected: boolean;
    disabled: boolean;
    onSelect: (optionId: string) => void;
}) => {
    return (
        <button
            type="button"
            onClick={() => onSelect(option.id)}
            disabled={disabled}
            className={`flex items-center gap-3 rounded-xl border px-4 py-3 text-left transition ${
                selected
                    ? "bg-emerald-50 border-emerald-500"
                    : "border-slate-200 bg-white hover:border-slate-300"
            }`}
        >
            <span className="flex h-6 w-6 items-center justify-center rounded-full border text-xs font-semibold text-slate-700">
                {label}
            </span>
            <span className="text-sm text-slate-700">{option.text}</span>
        </button>
    );
};

const ExerciseClientComponent = () => {
    console.log("Rendering ExerciseClientComponent");
    const params = useParams();
    const { data: session, status } = useSession();
    const topicSlug = normalizeRouteParam(params["slug-topic"]);
    const sessionIdFromRoute = normalizeRouteParam(params["session-id"]);
    const getSessionItemsSnapshotQuery = useGetSessionItemsSnapshot(
        sessionIdFromRoute,
        topicSlug,
    );
    const queryClient = useQueryClient();
    const saveAnswerMutation = useSaveAnswer();
    // đây là load lần đầu khi bắt đầu phiên học, sẽ trả về bài tập đầu tiên của session, sau đó sẽ ưu tiên lấy chi tiết bài tập từ cache của startLearningTopicResponse để tránh phải gọi API nhiều lần khi chuyển giữa các câu trong session
    const cachedStartOrContinueLearning = queryClient.getQueryData<
        BaseResponse<
            StartLearningTopicResponse | CountinueLearningSessionResponse
        >
    >(
        topicSlug
            ? startLearningTopicQueryKey(topicSlug)
            : ["start-learning-topic"],
    );
    // #region state & query

    const userId = session?.user?.id;
    const resolvedSessionId = useMemo(() => {
        return (
            sessionIdFromRoute ||
            cachedStartOrContinueLearning?.data?.sessionId ||
            ""
        );
    }, [cachedStartOrContinueLearning?.data?.sessionId, sessionIdFromRoute]);
    // child state
    // list
    const [currentSessionItems, setCurrentSessionItems] = useState<
        LearningExerciseSessionItemDTOResponse[] | null
    >(null);
    const [currentExercise, setCurrentExercise] =
        useState<ExerciseSessionPracticeItemWithoutAnswerResponse | null>(null);
    const [selectedExerciseId, setSelectedExerciseId] = useState<string>("");

    const [currentSequenceNumber, setCurrentSequenceNumber] = useState(0);
    const [totalExercises, setTotalExercises] = useState(0);
    // #region Effects: session bootstrap
    // khi mới chuyển trang vào, lấy từ cache của start hoặc continue learning response để có dữ liệu hiển thị ngay, sau đó sẽ gọi API lấy chi tiết bài tập mới nhất, nếu có sự thay đổi về bài tập (ví dụ do lỗi trước đó hoặc do cập nhật nội dung bài tập) thì sẽ cập nhật lại chi tiết bài tập, còn nếu không có sự thay đổi nào thì sẽ giữ nguyên để tránh nhấp nháy khi chuyển giữa các câu trong session
    useEffect(() => {
        console.log("Effect to load session data from cache or snapshot");
        if (
            cachedStartOrContinueLearning?.success &&
            cachedStartOrContinueLearning.data
        ) {
            const firstExerciseFromCache =
                cachedStartOrContinueLearning.data?.firstExercise;
            const sessionItemsFromCache =
                cachedStartOrContinueLearning.data?.sessionItems ?? [];
            // set dữ liệu từ cache để hiển thị ngay mà không cần chờ API, sau đó sẽ gọi API lấy chi tiết bài tập mới nhất để cập nhật nếu có sự thay đổi
            setCurrentSequenceNumber(
                cachedStartOrContinueLearning.data.currentSequenceNo ?? 0,
            );
            setTotalExercises(
                cachedStartOrContinueLearning.data.totalExercises ?? 0,
            );
            if (firstExerciseFromCache && !currentExercise) {
                setCurrentExercise(firstExerciseFromCache);
                setSelectedExerciseId(firstExerciseFromCache.exerciseId);
            }
            if (sessionItemsFromCache && !currentSessionItems) {
                setCurrentSessionItems(sessionItemsFromCache);
            }
        } else {
            // nếu không có dữ liệu nào trong cache, có thể do người dùng vừa tải lại trang,
            // thì sẽ lấy query param có slug và exerciseId để gọi API lấy chi tiết bài tập, đồng thời tạo dữ liệu giả tạm thời để hiển thị, tránh trường hợp không có dữ liệu nào để hiển thị
            if (topicSlug && sessionIdFromRoute) {
                getSessionItemsSnapshotQuery.refetch();
            }
        }
    }, [
        cachedStartOrContinueLearning,
        currentExercise,
        currentSessionItems,
        topicSlug,
        sessionIdFromRoute,
    ]);

    // Effect này sẽ ưu tiên cập nhật dữ liệu từ snapshot API sau khi đã load dữ liệu ban đầu từ cache, vì dữ liệu từ snapshot sẽ mới nhất và chính xác nhất, còn dữ liệu từ cache có thể đã cũ hoặc không còn hợp lệ nếu có sự thay đổi về bài tập trong session, nên sau khi có dữ liệu mới từ snapshot sẽ cập nhật lại chi tiết bài tập đang hiển thị nếu bài tập đó là bài tập đang được chọn, còn nếu không phải thì sẽ giữ nguyên để tránh nhấp nháy khi chuyển giữa các câu trong session
    useEffect(() => {
        if (
            cachedStartOrContinueLearning?.success &&
            cachedStartOrContinueLearning.data
        ) {
            return;
        }

        const snapshotData = getSessionItemsSnapshotQuery.data?.data;
        if (!snapshotData) {
            return;
        }

        setCurrentSequenceNumber(snapshotData.currentSequenceNo ?? 0);
        setTotalExercises(snapshotData.totalExercises ?? 0);

        const sessionItemsFromSnapshot = snapshotData.sessionItems ?? [];
        if (sessionItemsFromSnapshot.length > 0 && !currentSessionItems) {
            setCurrentSessionItems(sessionItemsFromSnapshot);
            if (!currentExercise) {
                setSelectedExerciseId(sessionItemsFromSnapshot[0].exerciseId);
            }
        }
    }, [
        cachedStartOrContinueLearning?.success,
        cachedStartOrContinueLearning?.data,
        currentExercise,
        currentSessionItems,
        getSessionItemsSnapshotQuery.data,
    ]);
    // #endregion

    const handleSubmit = useCallback(() => {
        if (!currentExercise || !currentExercise.exerciseId) {
            return;
        }
    }, [currentExercise]);
    useEffect(() => {
        if (!currentSessionItems || currentSessionItems.length === 0) {
            return;
        }
    }, [currentExercise]);

    const exerciseSessionPracticeQuery =
        useExerciseSessionPracticeWithoutAnswer(selectedExerciseId);

    // #region Effects: exercise detail sync
    useEffect(() => {
        if (!selectedExerciseId) {
            return;
        }
        // khi có dữ liệu bài tập mới từ API, sẽ cập nhật lại chi tiết bài tập đang hiển thị nếu bài tập đó là bài tập đang được chọn, còn nếu không phải thì sẽ giữ nguyên để tránh nhấp nháy khi chuyển giữa các câu trong session
        const nextExercise = exerciseSessionPracticeQuery.data?.data;
        if (nextExercise && nextExercise.exerciseId === selectedExerciseId) {
            setCurrentExercise(nextExercise);
        }
    }, [exerciseSessionPracticeQuery.data, selectedExerciseId]);
    // #endregion

    const [currentIndex, setCurrentIndex] = useState(0);
    // setAnswers sẽ lưu trữ câu trả lời của người dùng cho từng bài tập trong session, key là exerciseId để dễ dàng truy cập và cập nhật khi người dùng chọn đáp án hoặc nhập câu trả lời
    const [answers, setAnswers] = useState<Record<string, string>>({});
    // submittedAnswers sẽ lưu trữ trạng thái đã nộp của từng bài tập, key là exerciseId và value là boolean để biết được bài tập nào đã được nộp để hiển thị trạng thái và ngăn người dùng chỉnh sửa sau khi đã nộp
    const [submittedAnswers, setSubmittedAnswers] = useState<
        Record<string, boolean>
    >({});
    const totalExercisesValue = useMemo(() => {
        return currentSessionItems?.length ?? totalExercises;
    }, [currentSessionItems, totalExercises]);

    const isFinalQuestion = useMemo(() => {
        return currentIndex >= Math.max(totalExercisesValue - 1, 0);
    }, [currentIndex, totalExercisesValue]);
    const answeredCount =
        Object.values(submittedAnswers).filter(Boolean).length;
    const progressPercent =
        totalExercisesValue > 0
            ? Math.round((answeredCount / totalExercisesValue) * 100)
            : 0;

    const currentAnswer = currentExercise?.exerciseId
        ? (answers[currentExercise.exerciseId] ?? "")
        : "";

    const isCurrentSubmitted = currentExercise?.exerciseId
        ? Boolean(submittedAnswers[currentExercise.exerciseId])
        : false;

    const isLastQuestion = currentIndex >= Math.max(totalExercisesValue - 1, 0);

    const handleSelectOption = useCallback(
        (optionId: string) => {
            if (!currentExercise?.exerciseId || isCurrentSubmitted) {
                return;
            }

            setAnswers((prev) => ({
                ...prev,
                [currentExercise.exerciseId]: optionId,
            }));
        },
        [currentExercise?.exerciseId, isCurrentSubmitted],
    );

    const handleInputAnswer = useCallback(
        (value: string) => {
            if (!currentExercise?.exerciseId || isCurrentSubmitted) {
                return;
            }

            setAnswers((prev) => ({
                ...prev,
                [currentExercise.exerciseId]: value,
            }));
        },
        [currentExercise?.exerciseId, isCurrentSubmitted],
    );

    const handleNavigateTo = useCallback(
        (index: number, exerciseId: string) => {
            if (index < 0 || index >= totalExercisesValue) {
                return;
            }

            setCurrentIndex(index);
            setSelectedExerciseId(exerciseId);
        },
        [totalExercisesValue],
    );

    const handleNext = useCallback(() => {
        if (!currentExercise?.exerciseId) {
            return;
        }

        const nextItem = currentSessionItems?.[currentIndex + 1];
        if (!nextItem) {
            return;
        }

        if (!resolvedSessionId || !currentAnswer) {
            handleNavigateTo(currentIndex + 1, nextItem.exerciseId);
            return;
        }

        saveAnswerMutation.mutate(
            {
                sessionId: resolvedSessionId,
                exerciseId: currentExercise.exerciseId,
                answer: currentAnswer,
            },
            {
                onSuccess: () => {
                    setSubmittedAnswers((prev) => ({
                        ...prev,
                        [currentExercise.exerciseId]: true,
                    }));
                    handleNavigateTo(currentIndex + 1, nextItem.exerciseId);
                },
            },
        );
    }, [
        currentAnswer,
        currentExercise?.exerciseId,
        currentIndex,
        currentSessionItems,
        handleNavigateTo,
        resolvedSessionId,
        saveAnswerMutation,
    ]);

    if (status === "loading") {
        return (
            <div className="rounded-2xl border border-slate-200 bg-white p-4 text-sm text-slate-500 shadow-sm">
                Đang tải phiên học...
            </div>
        );
    }

    if (!topicSlug || !userId) {
        return (
            <div className="rounded-2xl border border-amber-200 bg-amber-50 p-4 text-sm text-amber-700 shadow-sm">
                Không tìm thấy dữ liệu phiên học. Hãy quay lại trang chủ đề và
                bắt đầu học lại.
            </div>
        );
    }

    const hasCachedSession = Boolean(
        cachedStartOrContinueLearning?.success &&
        cachedStartOrContinueLearning.data,
    );
    const hasSnapshotSession = Boolean(
        getSessionItemsSnapshotQuery.data?.data?.sessionItems?.length,
    );

    if (!hasCachedSession && !hasSnapshotSession) {
        return (
            <div className="rounded-2xl border border-slate-200 bg-white p-4 text-sm text-slate-600 shadow-sm">
                Chưa có dữ liệu phiên học trong bộ nhớ tạm. Nếu bạn vừa tải lại
                trang, hãy nhấn <span className="font-medium">Bắt đầu học</span>{" "}
                từ trang chủ đề để tạo phiên mới.
            </div>
        );
    }

    if (!currentExercise || !currentExercise) {
        return (
            <div className="rounded-2xl border border-amber-200 bg-amber-50 p-4 text-sm text-amber-700 shadow-sm">
                Phiên học chưa có bài tập hợp lệ.
            </div>
        );
    }

    if (!currentExercise) {
        return (
            <div className="rounded-2xl border border-red-200 bg-red-50 p-4 text-sm text-red-700 shadow-sm">
                Không thể tải chi tiết bài tập hiện tại. Vui lòng thử lại.
            </div>
        );
    }

    return (
        <div className="space-y-4 rounded-2xl border border-slate-200 bg-white p-4 shadow-sm">
            <header className="space-y-3 rounded-xl border border-slate-200 bg-slate-50 p-4">
                <div className="flex flex-wrap items-start justify-between gap-2">
                    <div>
                        <p className="text-xs font-semibold uppercase tracking-wide text-slate-500">
                            Phiên học
                        </p>
                        <h1 className="mt-1 text-base font-semibold text-slate-900">
                            Chủ đề: {topicSlug}
                        </h1>
                        <p className="mt-1 text-sm text-slate-600">
                            Câu {currentIndex + 1}/{totalExercisesValue} - Đã
                            nộp {answeredCount}/{totalExercisesValue}
                        </p>
                    </div>
                    <span className="rounded-lg bg-emerald-100 px-2.5 py-1 text-xs font-semibold text-emerald-700">
                        Tiến độ {progressPercent}%
                    </span>
                </div>

                <div className="h-2 w-full overflow-hidden rounded-full bg-slate-200">
                    <div
                        className="h-full rounded-full bg-emerald-500 transition-all"
                        style={{ width: `${progressPercent}%` }}
                    />
                </div>
            </header>

            <div className="grid grid-cols-3 place-content-center">
                <section className="space-y-3">
                    {currentSessionItems!.map((item, index) => {
                        const submitted = Boolean(
                            submittedAnswers[item.exerciseId],
                        );
                        const active = index === currentIndex;

                        return (
                            <button
                                key={item.sessionItemId}
                                type="button"
                                onClick={() =>
                                    handleNavigateTo(index, item.exerciseId)
                                }
                                className={`rounded-xl border px-3 py-2 text-left transition ${
                                    active
                                        ? ` bg-emerald-50 ${submitted ? "border-emerald-500" : "border-blue-300"}`
                                        : submitted
                                          ? "border-sky-300 bg-sky-50"
                                          : "border-slate-200 bg-white hover:border-slate-300"
                                }`}
                            >
                                <p className="text-xs font-semibold uppercase tracking-wide text-slate-500">
                                    Câu {index + 1}
                                </p>
                            </button>
                        );
                    })}
                </section>

                <section className="space-y-4 col-span-2 rounded-xl border border-slate-200 p-4">
                    <div className="space-y-2">
                        <div className="flex flex-wrap items-center gap-2 text-xs font-semibold text-slate-500">
                            <span className="rounded bg-slate-100 px-2 py-1">
                                {currentExercise.exerciseType}
                            </span>
                            <span className="rounded bg-slate-100 px-2 py-1">
                                {currentExercise.skillType}
                            </span>
                            <span className="rounded bg-slate-100 px-2 py-1">
                                {String(currentExercise.difficulty)}
                            </span>
                        </div>

                        <h2 className="text-lg font-semibold leading-7 text-slate-900">
                            Câu hỏi: {currentExercise.question}
                        </h2>
                        <p className="text-sm leading-6 text-slate-600">
                            Mô tả: {currentExercise.description}
                        </p>
                    </div>

                    {currentExercise.imageUrl ? (
                        <img
                            src={currentExercise.imageUrl}
                            alt="Exercise visual"
                            className="max-h-72 w-full rounded-lg object-contain"
                        />
                    ) : null}

                    {currentExercise.audioUrl ? (
                        <audio controls className="w-full">
                            <source src={currentExercise.audioUrl} />
                        </audio>
                    ) : null}

                    {currentExercise.options &&
                    currentExercise.options.length > 0 ? (
                        <div className="grid grid-cols-1 gap-2">
                            {currentExercise.options.map((option, index) => {
                                const selected = currentAnswer === option.id;
                                const label =
                                    optionLabels[index] ?? String(index + 1);

                                return (
                                    <ExerciseOptionButton
                                        key={option.id}
                                        option={option}
                                        label={label}
                                        selected={selected}
                                        disabled={isCurrentSubmitted}
                                        onSelect={handleSelectOption}
                                    />
                                );
                            })}
                        </div>
                    ) : (
                        <textarea
                            value={currentAnswer}
                            onChange={(e) => handleInputAnswer(e.target.value)}
                            disabled={isCurrentSubmitted}
                            placeholder="Nhập câu trả lời của bạn..."
                            className="min-h-24 w-full rounded-xl border border-slate-300 px-3 py-2 text-sm outline-none ring-emerald-500 transition focus:ring-2 disabled:cursor-not-allowed disabled:bg-slate-100"
                        />
                    )}

                    <div className="flex flex-wrap items-center justify-between gap-2 pt-2">
                        <div className="flex gap-2">
                            <Button
                                type="button"
                                variant="outline"
                                disabled={currentIndex === 0}
                                onClick={() =>
                                    handleNavigateTo(
                                        currentIndex - 1,
                                        currentSessionItems![currentIndex - 1]
                                            .exerciseId,
                                    )
                                }
                            >
                                Câu trước
                            </Button>
                            <Button
                                type="button"
                                variant="outline"
                                disabled={
                                    isLastQuestion ||
                                    saveAnswerMutation.isPending
                                }
                                onClick={() => {
                                    if (isFinalQuestion) {
                                        handleSubmit();
                                        return;
                                    }
                                    handleNext();
                                }}
                            >
                                {isLastQuestion
                                    ? "Nộp bài và hoàn thành"
                                    : "Câu tiếp"}
                            </Button>
                        </div>
                    </div>
                </section>
            </div>
        </div>
    );
};

export default ExerciseClientComponent;
