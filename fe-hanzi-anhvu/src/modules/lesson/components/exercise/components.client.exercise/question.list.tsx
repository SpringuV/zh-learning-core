"use client";

import { memo } from "react";
import { LearningExerciseSessionItemDTOResponse } from "@/modules/lesson/types/topic.type";

interface QuestionListProps {
    items: LearningExerciseSessionItemDTOResponse[];
    currentIndex: number;
    submittedAnswers: Record<string, boolean>;
    onNavigate: (index: number, exerciseId: string) => void;
}

const QuestionList = memo(
    ({
        items,
        currentIndex,
        submittedAnswers,
        onNavigate,
    }: QuestionListProps) => {
        return (
            <section className="space-y-2 px-2 w-full">
                <h2 className="font-bold">Danh sách câu hỏi</h2>
                <div className="grid grid-cols-3 gap-2">
                    {items.map((item, index) => {
                        const submitted = Boolean(
                            submittedAnswers[item.exerciseId],
                        );
                        const active = index === currentIndex;

                        return (
                            <button
                                key={item.sessionItemId}
                                type="button"
                                onClick={() =>
                                    onNavigate(index, item.exerciseId)
                                }
                                className={`rounded-xl border px-2 py-1 text-left transition mb-0 ${
                                    active
                                        ? ` bg-emerald-50 ${submitted ? "border-emerald-500" : "border-blue-300"}`
                                        : submitted
                                          ? "border-sky-300 bg-sky-50"
                                          : "border-slate-200 bg-white hover:border-slate-300"
                                }`}
                            >
                                <p className="text-xs font-semibold uppercase text-center tracking-wide text-slate-500">
                                    C{index + 1}
                                </p>
                            </button>
                        );
                    })}
                </div>
            </section>
        );
    },
);

// Set display name for better debugging and React DevTools identification
QuestionList.displayName = "QuestionList";

export default QuestionList;
