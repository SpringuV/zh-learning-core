"use client";

import { memo } from "react";

interface SessionHeaderProps {
    topicSlug: string;
    currentIndex: number;
    totalExercisesValue: number;
    answeredCount: number;
    progressPercent: number;
}

const SessionHeader = memo(
    ({
        topicSlug,
        currentIndex,
        totalExercisesValue,
        answeredCount,
        progressPercent,
    }: SessionHeaderProps) => {
        return (
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
        );
    },
);

SessionHeader.displayName = "SessionHeader";

export default SessionHeader;
