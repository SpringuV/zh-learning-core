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
import { GripVertical, Trash2 } from "lucide-react";
import { Badge } from "@/shared/components/ui/badge";
import { Switch } from "@/shared/components/ui/switch";
import { TopicListItemAdmin } from "@/modules/lesson/types/topic.type";

type TopicManagementTableProps = {
    topics: TopicListItemAdmin[];
    isReorderMode: boolean;
    isReorderPending: boolean;
    pendingTopicId: string | null;
    isLoading: boolean;
    isError: boolean;
    normalizedCourseId: string;
    onMoveTopic: (activeId: string, overId: string) => void;
    onTogglePublishTopic: (
        topicId: string,
        isPublished: boolean,
    ) => void | Promise<void>;
    onDeleteTopic: (topicId: string, title: string) => void;
};

type SortableTopicRowProps = {
    topic: TopicListItemAdmin;
    isReorderMode: boolean;
    isReorderPending: boolean;
    pendingTopicId: string | null;
    normalizedCourseId: string;
    onTogglePublishTopic: (
        topicId: string,
        isPublished: boolean,
    ) => void | Promise<void>;
    onDeleteTopic: (topicId: string, title: string) => void;
};

function SortableTopicRow({
    topic,
    isReorderMode,
    isReorderPending,
    pendingTopicId,
    normalizedCourseId,
    onTogglePublishTopic,
    onDeleteTopic,
}: SortableTopicRowProps) {
    const {
        attributes,
        listeners,
        setNodeRef,
        transform,
        transition,
        isDragging,
    } = useSortable({
        id: topic.id,
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
                            aria-label={`Kéo để sắp xếp ${topic.title}`}
                            {...attributes}
                            {...listeners}
                        >
                            <GripVertical className="h-4 w-4" />
                        </button>
                    )}
                    <span>#{topic.orderIndex}</span>
                </div>
            </td>
            <td className="px-6 py-4 font-medium text-slate-900">
                {topic.title}
            </td>
            <td className="hidden px-6 py-4 text-slate-500 sm:table-cell">
                {topic.totalExercises} bài tập
            </td>
            <td className="px-6 py-4">
                <div className="flex items-center gap-2">
                    <Switch
                        checked={topic.isPublished}
                        size="default"
                        className="h-6 w-11 border border-slate-300 ring-1 ring-slate-200 data-checked:bg-emerald-600 data-unchecked:bg-slate-300"
                        disabled={pendingTopicId === topic.id}
                        onCheckedChange={() =>
                            void onTogglePublishTopic(
                                topic.id,
                                topic.isPublished,
                            )
                        }
                        aria-label={`Chuyển trạng thái xuất bản của ${topic.title}`}
                    />
                    <Badge
                        className={
                            topic.isPublished
                                ? "bg-green-100 text-green-700"
                                : "bg-slate-100 text-slate-600"
                        }
                    >
                        {topic.isPublished ? "Đã xuất bản" : "Nháp"}
                    </Badge>
                </div>
            </td>
            <td className="px-6 py-4 text-right">
                {pendingTopicId === topic.id && (
                    <p className="mb-2 text-[11px] text-slate-500">
                        Đang xử lý...
                    </p>
                )}
                <div className="flex justify-end gap-2">
                    <button
                        type="button"
                        disabled={pendingTopicId === topic.id}
                        onClick={() => onDeleteTopic(topic.id, topic.title)}
                        className="inline-flex items-center gap-1 rounded border border-rose-200 px-2 py-1 text-xs text-rose-700 hover:bg-rose-50 disabled:cursor-not-allowed disabled:opacity-60"
                    >
                        <Trash2 className="h-3.5 w-3.5" />
                        Xóa
                    </button>

                    <Link
                        href={`/cms/lessons/course/${normalizedCourseId}/topics/${topic.id}`}
                    >
                        <button className="rounded border border-slate-200 bg-white px-3 py-1.5 text-xs text-slate-600 shadow-sm transition-colors hover:bg-slate-50 hover:text-amber-600">
                            Quản lý bài tập
                        </button>
                    </Link>
                </div>
            </td>
        </tr>
    );
}

export function TopicManagementTable({
    topics,
    isReorderMode,
    isReorderPending,
    pendingTopicId,
    isLoading,
    isError,
    normalizedCourseId,
    onMoveTopic,
    onTogglePublishTopic,
    onDeleteTopic,
}: TopicManagementTableProps) {
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

        onMoveTopic(String(active.id), String(over.id));
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
                            Tên Topic
                        </th>
                        <th className="hidden px-6 py-4 font-semibold text-slate-600 sm:table-cell">
                            Số bài tập
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
                                colSpan={5}
                            >
                                Đang tải topics...
                            </td>
                        </tr>
                    )}

                    {!isLoading && isError && (
                        <tr>
                            <td
                                className="px-6 py-6 text-center text-red-600"
                                colSpan={5}
                            >
                                Không thể tải danh sách topics.
                            </td>
                        </tr>
                    )}

                    {!isLoading && !isError && topics.length === 0 && (
                        <tr>
                            <td
                                className="px-6 py-6 text-center text-slate-500"
                                colSpan={5}
                            >
                                Chưa có topic nào trong khóa học này.
                            </td>
                        </tr>
                    )}

                    <SortableContext
                        items={topics.map((topic) => topic.id)}
                        strategy={verticalListSortingStrategy}
                    >
                        {!isLoading &&
                            !isError &&
                            topics.map((topic) => (
                                <SortableTopicRow
                                    key={topic.id}
                                    topic={topic}
                                    isReorderMode={isReorderMode}
                                    isReorderPending={isReorderPending}
                                    pendingTopicId={pendingTopicId}
                                    normalizedCourseId={normalizedCourseId}
                                    onTogglePublishTopic={onTogglePublishTopic}
                                    onDeleteTopic={onDeleteTopic}
                                />
                            ))}
                    </SortableContext>
                </tbody>
            </table>
        </DndContext>
    );
}
