"use client";

import { LearningExerciseSessionPracticeDTOResponse } from "@/modules/lesson/types/topic.type";
import { memo } from "react";

const optionLabels = "abcdefghijklmnopqrstuvwxyz".toUpperCase();

interface ExerciseContentProps {
    currentExercise: LearningExerciseSessionPracticeDTOResponse;
    currentAnswer: string;
    isCurrentSubmitted: boolean;
    onSelectOption: (optionId: string) => void;
    onInputAnswer: (value: string) => void;
}

const ExerciseContent = memo(
    ({
        currentExercise,
        currentAnswer,
        isCurrentSubmitted,
        onSelectOption,
        onInputAnswer,
    }: ExerciseContentProps) => {
        return (
            <>
                <div className="space-y-2">
                    <div className="flex flex-wrap items-center gap-2 text-xs font-semibold text-slate-500">
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
                        {currentExercise.description}
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
                    <audio
                        controls
                        className="w-full"
                        controlsList="nodownload"
                    >
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
                                <button
                                    key={option.id}
                                    type="button"
                                    disabled={isCurrentSubmitted}
                                    onClick={() => onSelectOption(option.id)}
                                    className={`rounded-xl border px-3 py-2 text-left text-sm transition ${
                                        selected
                                            ? "border-emerald-500 bg-emerald-50 text-emerald-800"
                                            : "border-slate-200 bg-white hover:border-slate-300"
                                    } ${isCurrentSubmitted ? "cursor-not-allowed opacity-80" : ""}`}
                                >
                                    <span className="mr-2 font-semibold">
                                        {label}.
                                    </span>
                                    {option.text}
                                </button>
                            );
                        })}
                    </div>
                ) : (
                    <textarea
                        value={currentAnswer}
                        onChange={(e) => onInputAnswer(e.target.value)}
                        disabled={isCurrentSubmitted}
                        placeholder="Nhập câu trả lời của bạn tại đây..."
                        className="min-h-24 w-full rounded-xl border border-slate-300 px-3 py-2 text-sm outline-none ring-emerald-500 transition focus:ring-2 disabled:cursor-not-allowed disabled:bg-slate-100"
                    />
                )}
            </>
        );
    },
);

ExerciseContent.displayName = "ExerciseContent";

export default ExerciseContent;
