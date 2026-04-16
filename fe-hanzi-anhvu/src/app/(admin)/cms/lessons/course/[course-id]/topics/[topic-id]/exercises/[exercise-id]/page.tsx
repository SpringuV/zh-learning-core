"use client";

import { useMemo, useState } from "react";
import Link from "next/link";
import { useParams } from "next/navigation";
import { useGetExerciseDetail } from "@/modules/lesson/hooks/use.exercise.tanstack";
import { useGetTopicDetail } from "@/modules/lesson/hooks/use.topic.tanstack";
import { Badge } from "@/shared/components/ui/badge";

// Mock Data matching ExerciseAggregate
const mockExercise = {
    id: "e1",
    topicId: "t1",
    courseId: "1",
    courseTitle: "HSK 1 - Starter",
    topicTitle: "Xin chào (Greetings)",
    description: "Nhận diện từ vựng cơ bản",
    exerciseType: "ListenDialogueChoice",
    skillType: "Listening",
    question: "Nghe đoạn audio và chọn từ đúng với nội dung bạn nghe được.",
    correctAnswer: "opt1",
    difficulty: "Medium",
    context: "Learning",
    audioUrl: "https://example.com/audio-sample.mp3",
    imageUrl: "",
    explanation: "Từ được phát âm là 'Nǐ hǎo' có nghĩa là xin chào.",
    status: "live",
    options: [
        { id: "opt1", text: "你好 (Nǐ hǎo)" },
        { id: "opt2", text: "再见 (Zàijiàn)" },
        { id: "opt3", text: "谢谢 (Xièxiè)" },
        { id: "opt4", text: "对不起 (Duìbùqǐ)" },
    ],
};

const skillTypeMap: Record<string, { label: string; color: string }> = {
    Listening: {
        label: "Nghe",
        color: "bg-blue-50 text-blue-700 border-blue-200",
    },
    Reading: {
        label: "Đọc",
        color: "bg-emerald-50 text-emerald-700 border-emerald-200",
    },
    Writing: {
        label: "Viết",
        color: "bg-amber-50 text-amber-700 border-amber-200",
    },
    Speaking: {
        label: "Nói",
        color: "bg-rose-50 text-rose-700 border-rose-200",
    },
};

const difficultyMap: Record<string, { label: string; color: string }> = {
    Easy: {
        label: "Dễ",
        color: "bg-emerald-50 text-emerald-700 border-emerald-200",
    },
    Medium: {
        label: "Trung bình",
        color: "bg-amber-50 text-amber-700 border-amber-200",
    },
    Hard: { label: "Khó", color: "bg-rose-50 text-rose-700 border-rose-200" },
};

export default function ExerciseDetailsPage() {
    const params = useParams();
    const courseId = params["course-id"];
    const topicId = params["topic-id"];
    const exerciseId = params["exercise-id"];

    const normalizedCourseId = useMemo(
        () =>
            Array.isArray(courseId)
                ? (courseId[0] ?? "")
                : String(courseId ?? ""),
        [courseId],
    );

    const normalizedTopicId = useMemo(
        () =>
            Array.isArray(topicId) ? (topicId[0] ?? "") : String(topicId ?? ""),
        [topicId],
    );

    const normalizedExerciseId = useMemo(
        () =>
            Array.isArray(exerciseId)
                ? (exerciseId[0] ?? "")
                : String(exerciseId ?? ""),
        [exerciseId],
    );

    const exerciseDetailQuery = useGetExerciseDetail(normalizedExerciseId);
    const topicDetailQuery = useGetTopicDetail(normalizedTopicId);

    const exercise = exerciseDetailQuery.data?.data;
    const topicDetail = topicDetailQuery.data?.data;

    const exerciseView = {
        ...mockExercise,
        courseId: normalizedCourseId || mockExercise.courseId,
        topicId: normalizedTopicId || mockExercise.topicId,
        id: normalizedExerciseId || mockExercise.id,
        courseTitle: `Course #${normalizedCourseId || "-"}`,
        topicTitle: topicDetail?.title ?? mockExercise.topicTitle,
        description: exercise?.description ?? mockExercise.description,
        exerciseType: exercise?.exerciseType ?? mockExercise.exerciseType,
        skillType: exercise?.skillType ?? mockExercise.skillType,
        question: exercise?.question ?? mockExercise.question,
        correctAnswer: exercise?.correctAnswer ?? mockExercise.correctAnswer,
        difficulty: exercise?.difficulty ?? mockExercise.difficulty,
        context: exercise?.context ?? mockExercise.context,
        audioUrl: exercise?.audioUrl ?? "",
        imageUrl: exercise?.imageUrl ?? "",
        explanation: exercise?.explanation ?? mockExercise.explanation,
        status: exercise?.isPublished ? "live" : "draft",
        options: exercise?.options?.length
            ? exercise.options
            : mockExercise.options,
    };

    const [activeTab, setActiveTab] = useState<"general" | "options">(
        "general",
    );

    return (
        <div className="-mx-6 -mt-6 min-h-screen bg-slate-50/50">
            {/* Header Section */}
            <div className="pt-8 px-8 border-b border-slate-200/50 bg-white/80 backdrop-blur-xl">
                <div className="flex flex-col md:flex-row justify-between gap-6 items-start md:items-center">
                    <div>
                        <div className="flex items-center gap-3 mb-2">
                            <Badge
                                variant="outline"
                                className="bg-slate-100 text-slate-600 border-slate-200"
                            >
                                {exerciseView.courseTitle} /{" "}
                                {exerciseView.topicTitle}
                            </Badge>
                            <Badge
                                variant="outline"
                                className={
                                    skillTypeMap[exerciseView.skillType]?.color
                                }
                            >
                                Kỹ năng:{" "}
                                {skillTypeMap[exerciseView.skillType]?.label}
                            </Badge>
                            <Badge
                                variant="outline"
                                className={
                                    difficultyMap[exerciseView.difficulty]
                                        ?.color
                                }
                            >
                                Mức độ:{" "}
                                {difficultyMap[exerciseView.difficulty]?.label}
                            </Badge>
                            <Badge
                                variant="secondary"
                                className={
                                    exerciseView.status === "live"
                                        ? "bg-emerald-100 text-emerald-700"
                                        : "bg-slate-100 text-slate-700"
                                }
                            >
                                {exerciseView.status === "live"
                                    ? "Đang hiển thị"
                                    : "Bản nháp"}
                            </Badge>
                        </div>
                        <h1 className="text-3xl font-semibold text-slate-900 tracking-tight">
                            Biên soạn bài tập
                        </h1>
                        <p className="text-slate-500 mt-2 max-w-2xl text-sm">
                            {exerciseView.description}
                        </p>
                        {exerciseDetailQuery.isLoading && (
                            <p className="mt-2 text-sm text-slate-500">
                                Đang tải chi tiết bài tập...
                            </p>
                        )}
                        {exerciseDetailQuery.isError && (
                            <p className="mt-2 text-sm text-red-600">
                                Không thể tải chi tiết bài tập.
                            </p>
                        )}
                    </div>

                    <div className="flex gap-3 shrink-0">
                        <Link
                            href={`/cms/lessons/course/${normalizedCourseId}/topics/${normalizedTopicId}`}
                        >
                            <button className="px-5 py-2.5 bg-white border border-slate-200 text-slate-700 rounded-lg hover:bg-slate-50 transition-colors font-medium text-sm">
                                Về chủ đề
                            </button>
                        </Link>
                        <button className="px-5 py-2.5 bg-amber-600 text-white rounded-lg hover:bg-amber-700 transition-colors font-medium text-sm">
                            Lưu thay đổi
                        </button>
                    </div>
                </div>

                {/* Tabs */}
                <div className="flex gap-8 pt-8">
                    {[
                        { id: "general", label: "Nội dung câu hỏi" },
                        { id: "options", label: "Tùy chọn & Đáp án" },
                    ].map((tab) => (
                        <button
                            key={tab.id}
                            onClick={() => setActiveTab(tab.id as any)}
                            className={`pb-4 text-sm font-medium transition-colors relative ${
                                activeTab === tab.id
                                    ? "text-amber-600"
                                    : "text-slate-500 hover:text-slate-800"
                            }`}
                        >
                            {tab.label}
                            {activeTab === tab.id && (
                                <div className="absolute bottom-0 left-0 right-0 h-0.5 bg-amber-600 z-10" />
                            )}
                        </button>
                    ))}
                </div>
            </div>

            {/* Tab Contents */}
            <div className="max-w-5xl mx-auto px-8 py-12">
                {activeTab === "general" && (
                    <div className="space-y-6">
                        <div className="bg-white rounded-xl border border-slate-200/50 shadow-sm p-8">
                            <h2 className="text-xl font-semibold text-slate-900 mb-6">
                                Thông tin chung
                            </h2>

                            <div className="space-y-6">
                                <div>
                                    <label className="block text-sm font-medium text-slate-700 mb-2">
                                        Mô tả ngắn (Description)
                                    </label>
                                    <input
                                        type="text"
                                        defaultValue={exerciseView.description}
                                        className="w-full px-4 py-2 bg-slate-50 border border-slate-200 rounded-lg text-slate-900 focus:outline-none focus:ring-2 focus:ring-amber-500/30 focus:border-amber-500/50 transition-all font-medium text-sm"
                                    />
                                </div>

                                <div className="grid grid-cols-2 gap-6">
                                    <div>
                                        <label className="block text-sm font-medium text-slate-700 mb-2">
                                            Loại bài tập (Type)
                                        </label>
                                        <select
                                            defaultValue={
                                                exerciseView.exerciseType
                                            }
                                            className="w-full px-4 py-2.5 bg-slate-50 border border-slate-200 rounded-lg text-slate-900 text-sm focus:outline-none focus:ring-2 focus:ring-amber-500/30 focus:border-amber-500/50 transition-all"
                                        >
                                            <option value="ListenDialogueChoice">
                                                Nghe hội thoại - Trắc nghiệm
                                            </option>
                                            <option value="ListenFillBlank">
                                                Nghe - Điền từ
                                            </option>
                                            <option value="ListenSentenceJudge">
                                                Nghe - Đúng/Sai
                                            </option>
                                            <option value="ReadFillBlank">
                                                Đọc - Điền từ
                                            </option>
                                            <option value="ReadComprehension">
                                                Đọc hiểu
                                            </option>
                                            <option value="ReadMatch">
                                                Đọc nối từ
                                            </option>
                                        </select>
                                    </div>
                                    <div>
                                        <label className="block text-sm font-medium text-slate-700 mb-2">
                                            Bối cảnh sử dụng
                                        </label>
                                        <select
                                            defaultValue={exerciseView.context}
                                            className="w-full px-4 py-2.5 bg-slate-50 border border-slate-200 rounded-lg text-slate-900 text-sm focus:outline-none focus:ring-2 focus:ring-amber-500/30 focus:border-amber-500/50 transition-all"
                                        >
                                            <option value="Learning">
                                                Tự học (Learning)
                                            </option>
                                            <option value="Classroom">
                                                Giao bài tập (Classroom)
                                            </option>
                                            <option value="Mixed">
                                                Hỗn hợp (Mixed)
                                            </option>
                                        </select>
                                    </div>
                                </div>
                                <div className="grid grid-cols-2 gap-6">
                                    <div>
                                        <label className="block text-sm font-medium text-slate-700 mb-2">
                                            Kỹ năng (Skill)
                                        </label>
                                        <select
                                            defaultValue={
                                                exerciseView.skillType
                                            }
                                            className="w-full px-4 py-2.5 bg-slate-50 border border-slate-200 rounded-lg text-slate-900 text-sm focus:outline-none focus:ring-2 focus:ring-amber-500/30 focus:border-amber-500/50 transition-all"
                                        >
                                            <option value="Listening">
                                                Listening (Nghe)
                                            </option>
                                            <option value="Reading">
                                                Reading (Đọc)
                                            </option>
                                            <option value="Writing">
                                                Writing (Viết)
                                            </option>
                                            <option value="Speaking">
                                                Speaking (Nói)
                                            </option>
                                        </select>
                                    </div>
                                    <div>
                                        <label className="block text-sm font-medium text-slate-700 mb-2">
                                            Độ khó (Difficulty)
                                        </label>
                                        <select
                                            defaultValue={
                                                exerciseView.difficulty
                                            }
                                            className="w-full px-4 py-2.5 bg-slate-50 border border-slate-200 rounded-lg text-slate-900 text-sm focus:outline-none focus:ring-2 focus:ring-amber-500/30 focus:border-amber-500/50 transition-all"
                                        >
                                            <option value="Easy">Dễ</option>
                                            <option value="Medium">
                                                Trung bình
                                            </option>
                                            <option value="Hard">Khó</option>
                                        </select>
                                    </div>
                                </div>
                            </div>
                        </div>

                        <div className="bg-white rounded-xl border border-slate-200/50 shadow-sm p-8">
                            <h2 className="text-xl font-semibold text-slate-900 mb-6">
                                Nội dung (Question & Media)
                            </h2>
                            <div className="space-y-6">
                                <div>
                                    <label className="block text-sm font-medium text-slate-700 mb-2">
                                        Đề bài (Question)
                                    </label>
                                    <textarea
                                        rows={4}
                                        defaultValue={exerciseView.question}
                                        className="w-full px-4 py-2.5 bg-slate-50 border border-slate-200 rounded-lg text-slate-900 text-sm focus:outline-none focus:ring-2 focus:ring-amber-500/30 focus:border-amber-500/50 transition-all resize-none"
                                    ></textarea>
                                </div>
                                <div className="grid grid-cols-2 gap-6">
                                    <div>
                                        <label className="block text-sm font-medium text-slate-700 mb-2">
                                            Audio URL
                                        </label>
                                        <input
                                            type="url"
                                            defaultValue={exerciseView.audioUrl}
                                            className="w-full px-4 py-2.5 bg-slate-50 border border-slate-200 rounded-lg text-slate-900 text-sm focus:outline-none focus:ring-2 focus:ring-amber-500/30 focus:border-amber-500/50 transition-all"
                                            placeholder="https://"
                                        />
                                    </div>
                                    <div>
                                        <label className="block text-sm font-medium text-slate-700 mb-2">
                                            Image URL
                                        </label>
                                        <input
                                            type="url"
                                            defaultValue={exerciseView.imageUrl}
                                            className="w-full px-4 py-2.5 bg-slate-50 border border-slate-200 rounded-lg text-slate-900 text-sm focus:outline-none focus:ring-2 focus:ring-amber-500/30 focus:border-amber-500/50 transition-all"
                                            placeholder="https://"
                                        />
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                )}

                {activeTab === "options" && (
                    <div className="space-y-6">
                        <div className="bg-white rounded-xl border border-slate-200/50 shadow-sm p-8">
                            <div className="flex justify-between items-center mb-6">
                                <div>
                                    <h2 className="text-xl font-semibold text-slate-900">
                                        Tùy chọn trắc nghiệm (Options)
                                    </h2>
                                    <p className="text-sm text-slate-500 mt-1">
                                        Lưu ý chọn ra một đáp án đúng (Correct
                                        Answer)
                                    </p>
                                </div>
                                <button className="px-4 py-2 bg-slate-100 text-slate-700 rounded-lg text-sm font-medium hover:bg-slate-200 transition-colors">
                                    + Thêm lựa chọn
                                </button>
                            </div>

                            <div className="space-y-3">
                                {exerciseView.options.map((option, index) => (
                                    <div
                                        key={option.id}
                                        className={`flex items-center gap-4 p-4 border rounded-lg transition-colors ${exerciseView.correctAnswer === option.id ? "border-amber-400 bg-amber-50/30" : "border-slate-200 bg-white"}`}
                                    >
                                        <div className="flex items-center gap-2 shrink-0">
                                            <input
                                                type="radio"
                                                name="correctAnswer"
                                                defaultChecked={
                                                    exerciseView.correctAnswer ===
                                                    option.id
                                                }
                                                className="w-4 h-4 text-amber-600 focus:ring-amber-500 border-slate-300"
                                            />
                                            <span className="text-sm font-medium text-slate-500 w-6">
                                                #{index + 1}
                                            </span>
                                        </div>
                                        <input
                                            type="text"
                                            defaultValue={option.text}
                                            className="flex-1 px-3 py-1.5 bg-transparent border border-slate-200 rounded text-slate-900 text-sm focus:outline-none focus:border-amber-500"
                                        />
                                        <button className="text-slate-400 hover:text-red-500 transition-colors p-2">
                                            <svg
                                                xmlns="http://www.w3.org/2000/svg"
                                                className="h-4 w-4"
                                                fill="none"
                                                viewBox="0 0 24 24"
                                                stroke="currentColor"
                                            >
                                                <path
                                                    strokeLinecap="round"
                                                    strokeLinejoin="round"
                                                    strokeWidth={2}
                                                    d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"
                                                />
                                            </svg>
                                        </button>
                                    </div>
                                ))}
                            </div>
                        </div>

                        <div className="bg-white rounded-xl border border-slate-200/50 shadow-sm p-8">
                            <h2 className="text-xl font-semibold text-slate-900 mb-6">
                                Giải thích & Phản hồi (Explanation)
                            </h2>
                            <div>
                                <textarea
                                    rows={4}
                                    defaultValue={exerciseView.explanation}
                                    className="w-full px-4 py-2.5 bg-slate-50 border border-slate-200 rounded-lg text-slate-900 text-sm focus:outline-none focus:ring-2 focus:ring-amber-500/30 focus:border-amber-500/50 transition-all resize-none"
                                    placeholder="Điền giải thích tại sao lại chọn đáp án đó..."
                                ></textarea>
                            </div>
                        </div>
                    </div>
                )}
            </div>
        </div>
    );
}
