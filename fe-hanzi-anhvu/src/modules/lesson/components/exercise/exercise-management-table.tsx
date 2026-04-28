import {
    closestCenter,
    DndContext,
    type DragEndEvent,
    PointerSensor,
    useSensor,
    useSensors,
} from "@dnd-kit/core";
import {
    SortableContext,
    useSortable,
    verticalListSortingStrategy,
} from "@dnd-kit/sortable";
import { CSS } from "@dnd-kit/utilities";
import Link from "next/link";
import { BookA, GripVertical, Trash2 } from "lucide-react";
import { Badge } from "@/shared/components/ui/badge";
import { Switch } from "@/shared/components/ui/switch";
import {
    ExerciseListItem,
    ExerciseType,
    SkillType,
} from "@/modules/lesson/types/exercise.type";

type ExerciseManagementTableProps = {
    exercises: ExerciseListItem[];
    startIndex: number;
    isReorderMode: boolean;
    isReorderPending: boolean;
    pendingExerciseId: string | null;
    isLoading: boolean;
    isError: boolean;
    normalizedCourseId: string;
    normalizedTopicId: string;
    exerciseTypeLabels: Record<ExerciseType, string>;
    skillLabels: Record<SkillType, string>;
    onMoveExercise: (activeId: string, overId: string) => void;
    onTogglePublishExercise: (
        exerciseId: string,
        isPublished: boolean,
    ) => void | Promise<void>;
    onDeleteExercise: (exerciseId: string, question: string) => void;
};

type SortableExerciseRowProps = {
    exercise: ExerciseListItem;
    startIndex: number;
    index: number;
    isReorderMode: boolean;
    isReorderPending: boolean;
    pendingExerciseId: string | null;
    normalizedCourseId: string;
    normalizedTopicId: string;
    exerciseTypeLabels: Record<ExerciseType, string>;
    skillLabels: Record<SkillType, string>;
    onTogglePublishExercise: (
        exerciseId: string,
        isPublished: boolean,
    ) => void | Promise<void>;
    onDeleteExercise: (exerciseId: string, question: string) => void;
};

function SortableExerciseRow({
    exercise,
    startIndex,
    index,
    isReorderMode,
    isReorderPending,
    pendingExerciseId,
    normalizedCourseId,
    normalizedTopicId,
    exerciseTypeLabels,
    skillLabels,
    onTogglePublishExercise,
    onDeleteExercise,
}: SortableExerciseRowProps) {
    const {
        attributes,
        listeners,
        setNodeRef,
        transform,
        transition,
        isDragging,
    } = useSortable({
        id: exercise.exerciseId,
        disabled: !isReorderMode || isReorderPending,
    });
    const style = {
        transform: CSS.Transform.toString(transform),
        transition,
    };

    return (
        <tr
            ref={setNodeRef}
            style={style}
            className={`group border-b border-slate-50 transition-colors hover:bg-slate-50/50 ${
                isDragging ? "relative z-10 bg-slate-100" : ""
            }`}
        >
            <td className="px-6 py-4 text-center font-medium text-slate-400">
                <div className="flex items-center justify-center gap-2">
                    {isReorderMode && (
                        <button
                            type="button"
                            className="touch-none cursor-grab text-slate-400 active:cursor-grabbing"
                            aria-label={`Kéo để sắp xếp ${exercise.question}`}
                            {...attributes}
                            {...listeners}
                        >
                            <GripVertical className="h-4 w-4" />
                        </button>
                    )}
                    <span>#{startIndex + index + 1}</span>
                </div>
            </td>
            <td className="px-6 py-4 font-medium text-slate-900">
                {exercise.question}
            </td>
            <td className="px-6 py-4 text-slate-600">
                {exerciseTypeLabels[exercise.exerciseType] ??
                    exercise.exerciseType}
            </td>
            <td className="px-6 py-4 text-slate-600">
                {skillLabels[exercise.skillType] ?? exercise.skillType}
            </td>
            <td className="px-6 py-4 text-slate-600">{exercise.difficulty}</td>
            <td className="px-6 py-4">
                <div className="flex items-center gap-2">
                    <Switch
                        checked={exercise.isPublished}
                        size="default"
                        className="h-6 w-11 border border-slate-300 ring-1 ring-slate-200 data-checked:bg-emerald-600 data-unchecked:bg-slate-300"
                        disabled={pendingExerciseId === exercise.exerciseId}
                        onCheckedChange={() =>
                            void onTogglePublishExercise(
                                exercise.exerciseId,
                                exercise.isPublished,
                            )
                        }
                        aria-label={`Chuyển trạng thái xuất bản của ${exercise.question}`}
                    />
                    <Badge
                        className={
                            exercise.isPublished
                                ? "bg-green-100 text-green-700"
                                : "bg-slate-100 text-slate-600"
                        }
                    >
                        {exercise.isPublished ? "Đã xuất bản" : "Nháp"}
                    </Badge>
                </div>
            </td>
            <td className="px-6 py-4 text-right">
                {pendingExerciseId === exercise.exerciseId && (
                    <p className="mb-2 text-[11px] text-slate-500">
                        Đang xử lý...
                    </p>
                )}
                <div className="flex justify-end gap-2">
                    <button
                        type="button"
                        disabled={pendingExerciseId === exercise.exerciseId}
                        onClick={() =>
                            onDeleteExercise(
                                exercise.exerciseId,
                                exercise.question,
                            )
                        }
                        className="inline-flex items-center gap-1 rounded border border-rose-200 px-2 py-1 text-xs text-rose-700 hover:bg-rose-50 disabled:cursor-not-allowed disabled:opacity-60"
                    >
                        <Trash2 className="h-3.5 w-3.5" />
                    </button>

                    <Link
                        href={`/cms/lessons/course/${normalizedCourseId}/topics/${normalizedTopicId}/exercises/${exercise.exerciseId}`}
                        className="inline-flex items-center rounded border border-slate-200 bg-white px-2 py-1 text-xs text-slate-600 shadow-sm transition-colors hover:bg-slate-50 hover:text-amber-600"
                    >
                        <BookA className="h-3.5 w-3.5" />
                    </Link>
                </div>
            </td>
        </tr>
    );
}

export function ExerciseManagementTable({
    exercises,
    startIndex,
    isReorderMode,
    isReorderPending,
    pendingExerciseId,
    isLoading,
    isError,
    normalizedCourseId,
    normalizedTopicId,
    exerciseTypeLabels,
    skillLabels,
    onMoveExercise,
    onTogglePublishExercise,
    onDeleteExercise,
}: ExerciseManagementTableProps) {
    const sensors = useSensors(
        useSensor(PointerSensor, {
            activationConstraint: {
                distance: 8,
            },
        }),
    );

    const handleDragEnd = ({ active, over }: DragEndEvent) => {
        if (!isReorderMode || !over || active.id === over.id) {
            return;
        }

        onMoveExercise(String(active.id), String(over.id));
    };

    return (
        <DndContext
            sensors={sensors}
            collisionDetection={closestCenter}
            onDragEnd={handleDragEnd}
        >
            <table className="w-full min-w-full text-left text-sm">
                <thead className="border-b sticky top-0  border-slate-200/50 bg-white backdrop-blur-sm z-10">
                    <tr>
                        <th className="w-16 px-6 py-4 text-center font-semibold text-slate-600">
                            STT
                        </th>
                        <th className="px-6 py-4 font-semibold text-slate-600">
                            Câu hỏi
                        </th>
                        <th className="px-6 py-4 font-semibold text-slate-600">
                            Loại
                        </th>
                        <th className="px-6 py-4 font-semibold text-slate-600">
                            Kỹ năng
                        </th>
                        <th className="px-6 py-4 font-semibold text-slate-600">
                            Độ khó
                        </th>
                        <th className="px-6 py-4 font-semibold text-slate-600">
                            Trạng thái
                        </th>
                        <th className="px-6 py-4 text-right font-semibold text-slate-600">
                            Thao tác
                        </th>
                    </tr>
                </thead>

                <tbody>
                    {isLoading && (
                        <tr>
                            <td
                                className="px-6 py-6 text-center text-slate-500"
                                colSpan={7}
                            >
                                Đang tải bài tập...
                            </td>
                        </tr>
                    )}

                    {!isLoading && isError && (
                        <tr>
                            <td
                                className="px-6 py-6 text-center text-red-600"
                                colSpan={7}
                            >
                                Không thể tải danh sách bài tập.
                            </td>
                        </tr>
                    )}

                    {!isLoading && !isError && exercises.length === 0 && (
                        <tr>
                            <td
                                className="px-6 py-6 text-center text-slate-500"
                                colSpan={7}
                            >
                                Chưa có bài tập nào trong topic này.
                            </td>
                        </tr>
                    )}

                    <SortableContext
                        items={exercises.map((exercise) => exercise.exerciseId)}
                        strategy={verticalListSortingStrategy}
                    >
                        {!isLoading &&
                            !isError &&
                            exercises.map((exercise, index) => (
                                <SortableExerciseRow
                                    key={exercise.exerciseId}
                                    exercise={exercise}
                                    startIndex={startIndex}
                                    index={index}
                                    isReorderMode={isReorderMode}
                                    isReorderPending={isReorderPending}
                                    pendingExerciseId={pendingExerciseId}
                                    normalizedCourseId={normalizedCourseId}
                                    normalizedTopicId={normalizedTopicId}
                                    exerciseTypeLabels={exerciseTypeLabels}
                                    skillLabels={skillLabels}
                                    onTogglePublishExercise={
                                        onTogglePublishExercise
                                    }
                                    onDeleteExercise={onDeleteExercise}
                                />
                            ))}
                    </SortableContext>
                </tbody>
            </table>
        </DndContext>
    );
}
