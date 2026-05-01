"use client";

import {
    startLearningTopicQueryKey,
    resultCompleteSessionQueryKey,
} from "@/modules/lesson/hooks/use.topic.tanstack";
import {
    CompleteLearningSessionResponse,
    CountinueLearningSessionResponse,
    LearningExerciseSessionItemDTOResponse,
    LearningExerciseSessionPracticeDTOResponse,
    StartLearningTopicResponse,
} from "@/modules/lesson/types/topic.type";
import NavigationActions from "@/modules/lesson/components/exercise/components.client.exercise/navigation.actions";
import QuestionList from "@/modules/lesson/components/exercise/components.client.exercise/question.list";
import SessionHeader from "@/modules/lesson/components/exercise/components.client.exercise/session.header";
import { BaseResponse } from "@/shared/types/store.type";
import { useQueryClient } from "@tanstack/react-query";
import { useParams, useRouter } from "next/navigation";
import { useSession } from "next-auth/react";
import { useCallback, useEffect, useMemo, useState } from "react";
import {
    useExerciseSessionPracticeWithoutAnswer,
    useGetSessionItemsSnapshot,
    useSaveAnswer,
    useSubmitSession,
} from "@/modules/lesson/hooks/use.exercise.tanstack";
import ExerciseContent from "@/modules/lesson/components/exercise/components.client.exercise/exercise.content";
import { toast } from "sonner";

const normalizeRouteParam = (param: string | string[] | undefined): string => {
    if (Array.isArray(param)) {
        return param[0] || "";
    }
    return param || "";
};

const ExerciseClientComponent = () => {
    // nếu người dùng reload lại page thì cảnh báo là tiến trình piên học sẽ lưu lại,
    // và khi họ quay lại trang chủ đề và nhấn bắt đầu học lại thì sẽ tiếp tục phiên học cũ
    // thay vì tạo phiên học mới, để tránh mất tiến trình học của người dùng, tuy nhiên nếu
    // người dùng đã hoàn thành phiên học hoặc muốn bắt đầu phiên học mới thì vẫn có thể bắt đầu lại
    // bình thường, nên ở đây sẽ chỉ hiển thị cảnh báo khi người dùng reload lại page trong quá trình
    // đang học mà chưa hoàn thành phiên học, còn nếu đã hoàn thành phiên học hoặc chưa bắt đầu học mà
    // reload lại page thì sẽ không hiển thị cảnh báo vì lúc đó không có tiến trình nào bị mất cả
    const params = useParams();
    const router = useRouter();
    const { data: session, status } = useSession();
    const topicSlug = normalizeRouteParam(params["slug-topic"]);
    const sessionIdFromRoute = normalizeRouteParam(params["session-id"]);
    const courseSlug = useMemo(
        () => String(params["slug"] ?? ""),
        [params["slug"]],
    );
    const getSessionItemsSnapshotQuery = useGetSessionItemsSnapshot(
        sessionIdFromRoute,
        topicSlug,
    );
    const submitSessionCompleteMutation = useSubmitSession();
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
        useState<LearningExerciseSessionPracticeDTOResponse | null>(null);
    const [selectedExerciseId, setSelectedExerciseId] = useState<string>("");

    const [currentSequenceNumber, setCurrentSequenceNumber] = useState(0);
    const [totalExercises, setTotalExercises] = useState(0);
    const [isInitialized, setIsInitialized] = useState(false);
    // #endregion
    // #region unload confirmation
    const handleReloadConfirmation = useCallback(
        (event: KeyboardEvent) => {
            if (!resolvedSessionId || !currentExercise) {
                return;
            }

            const isReloadKey =
                event.key === "F5" ||
                ((event.key === "r" || event.key === "R") &&
                    (event.ctrlKey || event.metaKey));
            if (!isReloadKey) {
                return;
            }

            event.preventDefault();
            if (confirm("Do you want to reload the page?")) {
                window.location.reload();
            }
        },
        [resolvedSessionId, currentExercise],
    );

    useEffect(() => {
        const handleBeforeUnload = (event: BeforeUnloadEvent) => {
            if (!resolvedSessionId || !currentExercise) {
                return;
            }

            event.preventDefault();
        };

        window.addEventListener("beforeunload", handleBeforeUnload);
        window.addEventListener("keydown", handleReloadConfirmation);

        return () => {
            window.removeEventListener("beforeunload", handleBeforeUnload);
            window.removeEventListener("keydown", handleReloadConfirmation);
        };
    }, [resolvedSessionId, currentExercise, handleReloadConfirmation]);
    // #endregion
    // #region Effects: session bootstrap
    // Load dữ liệu ban đầu từ cache hoặc chờ snapshot query tự động trigger
    useEffect(() => {
        if (isInitialized) return; // Chỉ chạy 1 lần

        console.log("Effect to load session data from cache or snapshot");
        if (
            cachedStartOrContinueLearning?.success &&
            cachedStartOrContinueLearning.data
        ) {
            const firstExerciseFromCache =
                cachedStartOrContinueLearning.data?.firstExercise;
            const sessionItemsFromCache =
                cachedStartOrContinueLearning.data?.sessionItems ?? [];

            setCurrentSequenceNumber(
                cachedStartOrContinueLearning.data.currentSequenceNo ?? 0,
            );
            setTotalExercises(
                cachedStartOrContinueLearning.data.totalExercises ?? 0,
            );
            if (firstExerciseFromCache) {
                setCurrentExercise(firstExerciseFromCache);
                setSelectedExerciseId(firstExerciseFromCache.exerciseId);
            }
            if (sessionItemsFromCache) {
                setCurrentSessionItems(sessionItemsFromCache);
            }
            setIsInitialized(true);
        }
    }, []);
    // Nếu không có cache, snapshot query sẽ tự động trigger khi sessionIdFromRoute + topicSlug có giá trị
    // vì hook đã có enabled: Boolean(sessionId) && Boolean(slugTopic)

    // Cập nhật dữ liệu từ snapshot khi có data
    useEffect(() => {
        if (
            cachedStartOrContinueLearning?.success &&
            cachedStartOrContinueLearning.data
        ) {
            return; // Nếu đã có cache, bỏ qua snapshot
        }

        const snapshotData = getSessionItemsSnapshotQuery.data?.data;
        if (!snapshotData) {
            return;
        }

        setCurrentSequenceNumber(snapshotData.currentSequenceNo ?? 0);
        setTotalExercises(snapshotData.totalExercises ?? 0);

        const sessionItemsFromSnapshot = snapshotData.sessionItems ?? [];
        if (sessionItemsFromSnapshot.length > 0) {
            setCurrentSessionItems(sessionItemsFromSnapshot);
            setSelectedExerciseId(sessionItemsFromSnapshot[0].exerciseId);
            setIsInitialized(true);
        }
    }, [
        cachedStartOrContinueLearning?.success,
        getSessionItemsSnapshotQuery.data?.data,
    ]);
    // #endregion

    const exerciseSessionPracticeQuery =
        useExerciseSessionPracticeWithoutAnswer(
            selectedExerciseId,
            resolvedSessionId,
        );

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
        // điều hướng các câu sẽ tự động lưu kết quả câu hiện tại trước khi chuyển sang câu tiếp theo,
        // để tránh trường hợp người dùng bỏ sót câu trả lời mà vẫn có thể chuyển sang bài tập tiếp theo,
        // tuy nhiên nếu chưa có sessionId hợp lệ (có thể do lỗi khi bắt đầu phiên học hoặc do người dùng
        // vừa tải lại trang mà chưa nhấn bắt đầu học lại) thì sẽ không lưu câu trả lời mà chỉ chuyển sang
        // bài tập tiếp theo, tránh trường hợp người dùng bị kẹt ở bài tập hiện tại mà không thể chuyển sang
        //  bài tập tiếp theo được
        async (index: number, exerciseId: string) => {
            if (index < 0 || index >= totalExercisesValue) {
                return;
            }
            if (
                currentAnswer === undefined ||
                !resolvedSessionId ||
                currentAnswer === ""
            ) {
                // nếu chưa có câu trả lời nào được chọn hoặc nhập, hoặc chưa có sessionId hợp lệ, thì sẽ không lưu và chỉ chuyển sang bài tập tiếp theo, tránh trường hợp người dùng bỏ sót câu trả lời mà vẫn có thể chuyển sang bài tập tiếp theo
                setCurrentIndex(index);
                setSelectedExerciseId(exerciseId);
                return;
            }
            await saveAnswerMutation.mutateAsync(
                {
                    sessionId: resolvedSessionId,
                    exerciseId: currentExercise!.exerciseId,
                    answer: currentAnswer,
                },
                {
                    onSuccess: () => {
                        setSubmittedAnswers((prev) => ({
                            ...prev,
                            [currentExercise!.exerciseId]: true, // đánh dấu bài tập hiện tại đã được nộp trước khi chuyển sang bài tập tiếp theo
                        }));
                        setCurrentIndex(index);
                        setSelectedExerciseId(exerciseId);
                    },
                    onError: () => {
                        // nếu lưu bài tập thất bại, thì vẫn cho phép điều hướng sang bài tập tiếp theo, tránh trường hợp người dùng bị kẹt ở bài tập hiện tại mà không thể chuyển sang bài tập tiếp theo được
                        setCurrentIndex(index);
                        setSelectedExerciseId(exerciseId);
                    },
                },
            );
        },
        [totalExercisesValue],
    );

    const handleSubmit = useCallback(async () => {
        if (!currentExercise || !currentExercise.exerciseId) {
            return;
        }
        // nộp câu trả lời của bài tập hiện tại trước khi submit toàn bộ session, để tránh trường hợp người dùng bỏ sót câu trả lời của bài tập cuối cùng mà vẫn có thể submit session được
        if (
            !resolvedSessionId ||
            currentAnswer === undefined ||
            currentAnswer === ""
        ) {
            alert(
                "Bạn cần hoàn thành câu trả lời của bài tập hiện tại trước khi nộp bài.",
            );
            return;
        } else {
            // step 1: lưu câu trả lời bài tập hiện tại và cuối cùng trong session
            const response = await saveAnswerMutation.mutateAsync(
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
                        // sau khi nộp câu trả lời của bài tập cuối cùng, sẽ gọi API để submit toàn bộ session, có thể là API hoàn thành phiên học hoặc API nộp bài tập tùy vào yêu cầu nghiệp vụ, ở đây tạm thời sẽ gọi API hoàn thành phiên học để đánh dấu phiên học đã hoàn thành, còn việc chấm điểm và hiển thị kết quả sẽ được
                        // xử lý ở trang kết quả sau khi hoàn thành phiên học
                    },
                },
            );
            console.log(
                "Save answer response before submitting session:",
                response,
            );
            if (!response) {
                // nếu không có response nào từ API lưu câu trả lời của bài tập cuối cùng, thì sẽ hiển thị cảnh báo lỗi và không gọi API để submit toàn bộ session, tránh trường hợp người dùng bị kẹt ở bài tập cuối cùng mà không thể submit được, tuy nhiên sẽ hiển thị cảnh báo để người dùng biết rằng có lỗi xảy ra khi lưu câu trả lời của bài tập cuối cùng và sẽ không thể submit session được
                alert(
                    "Có lỗi xảy ra khi lưu câu trả lời của bài tập cuối cùng. Không thể nộp bài.",
                );
                return;
            }
            // step 2: gọi API để submit toàn bộ session, có thể là API hoàn thành phiên học hoặc API nộp bài tập tùy vào yêu cầu nghiệp vụ, ở đây tạm thời sẽ gọi API hoàn thành phiên học để đánh dấu phiên học đã hoàn thành, còn việc chấm điểm và hiển thị kết quả sẽ được xử lý ở trang kết quả sau khi hoàn thành phiên học
            await submitSessionCompleteMutation.mutateAsync(
                {
                    sessionId: resolvedSessionId,
                },
                {
                    onSuccess: (
                        data: BaseResponse<CompleteLearningSessionResponse>,
                    ) => {
                        // sau khi hoàn thành phiên học thành công, sẽ điều hướng sang trang kết quả của phiên học để hiển thị kết quả chi tiết
                        toast.success(
                            "Hoàn thành phiên học thành công! Đang chuyển đến trang kết quả...",
                        );
                        queryClient.setQueryData(
                            resultCompleteSessionQueryKey(resolvedSessionId),
                            data,
                        );
                        router.push(
                            `/u/${courseSlug}/${topicSlug}/${resolvedSessionId}/result`,
                        );
                    },
                    onError: () => {
                        // nếu hoàn thành phiên học thất bại, thì sẽ hiển thị cảnh báo lỗi và không điều hướng đi đâu cả để người dùng có thể thử lại, vì nếu đã hoàn thành phiên học thành công thì mới nên điều hướng sang trang kết quả, còn nếu hoàn thành phiên học thất bại mà vẫn điều hướng sang trang kết
                        // quả thì sẽ gây nhầm lẫn cho người dùng vì họ sẽ nghĩ rằng phiên học đã được hoàn thành thành công trong khi thực tế là đã có lỗi xảy ra và phiên học vẫn chưa được hoàn thành, nên ở đây sẽ chỉ hiển thị cảnh báo lỗi để người dùng biết rằng có lỗi xảy ra khi hoàn thành phiên học và họ có thể thử lại, thay vì điều hướng sang trang kết quả mà
                        toast.error(
                            "Có lỗi xảy ra khi hoàn thành phiên học. Vui lòng thử lại.",
                        );
                    },
                },
            );
        }
    }, [
        currentAnswer,
        currentExercise,
        courseSlug,
        resolvedSessionId,
        submitSessionCompleteMutation,
        topicSlug,
    ]);

    const handleNext = useCallback(async () => {
        if (!currentExercise?.exerciseId) {
            return;
        }

        const nextItem = currentSessionItems?.[currentIndex + 1];
        if (!nextItem) {
            return;
        }
        if (
            currentAnswer === undefined ||
            !resolvedSessionId || // nếu chưa có sessionId hợp lệ, có thể do lỗi khi bắt đầu phiên học hoặc do người dùng vừa tải lại trang mà chưa nhấn bắt đầu học lại, thì sẽ không lưu câu trả lời mà chỉ chuyển sang bài tập tiếp theo, tránh trường hợp người dùng bị kẹt ở bài tập hiện tại mà không thể chuyển sang bài tập tiếp theo được
            currentAnswer === ""
        ) {
            // nếu chưa có câu trả lời nào được chọn hoặc nhập, thì sẽ không lưu và chỉ chuyển sang bài tập tiếp theo, tránh trường hợp người dùng bỏ sót câu trả lời mà vẫn có thể chuyển sang bài tập tiếp theo
            handleNavigateTo(currentIndex + 1, nextItem.exerciseId);
            return;
        }

        await saveAnswerMutation.mutateAsync(
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
        if (!saveAnswerMutation.isSuccess) {
            // nếu không đang trong trạng thái lưu bài tập, thì cho phép chuyển sang bài tập tiếp theo, tránh trường hợp click nhanh nhiều
            handleNavigateTo(currentIndex + 1, nextItem.exerciseId);
        }
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
        <div className="space-y-4 rounded-2xl border border-slate-200 bg-white shadow-sm">
            <SessionHeader
                answeredCount={answeredCount}
                currentIndex={currentIndex}
                progressPercent={progressPercent}
                topicSlug={topicSlug}
                totalExercisesValue={totalExercisesValue}
            />

            <div className="grid grid-cols-3 place-content-center">
                <QuestionList
                    currentIndex={currentIndex}
                    items={currentSessionItems!}
                    submittedAnswers={submittedAnswers}
                    onNavigate={handleNavigateTo}
                />

                <section className="space-y-4 col-span-2 rounded-xl border border-slate-200 p-4">
                    <ExerciseContent
                        currentAnswer={currentAnswer}
                        currentExercise={currentExercise}
                        isCurrentSubmitted={isCurrentSubmitted}
                        onInputAnswer={handleInputAnswer}
                        onSelectOption={handleSelectOption}
                    />
                    <NavigationActions
                        currentIndex={currentIndex}
                        isFinalQuestion={isFinalQuestion}
                        isPending={saveAnswerMutation.isPending}
                        isLastQuestion={isLastQuestion}
                        onNext={handleNext}
                        onPrev={() =>
                            handleNavigateTo(
                                currentIndex - 1,
                                currentSessionItems![currentIndex - 1]
                                    .exerciseId,
                            )
                        }
                        onSubmit={() => {
                            handleSubmit();
                        }}
                    />
                </section>
            </div>
        </div>
    );
};

export default ExerciseClientComponent;
