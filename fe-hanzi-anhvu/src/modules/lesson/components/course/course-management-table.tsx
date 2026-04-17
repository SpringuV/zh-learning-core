import { memo } from "react";
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
import { type CourseListItem } from "@/modules/lesson/types/coure.type";

type CourseManagementTableProps = {
    courses: CourseListItem[];
    isReorderMode: boolean;
    pendingCourseId: string | null;
    isReorderPending: boolean;
    isLoading: boolean;
    isError: boolean;
    onMoveCourse: (activeId: string, overId: string) => void;
    onOpenPublishDialog: (
        courseId: string,
        courseTitle: string,
        nextPublished: boolean,
    ) => void;
    onDeleteCourse: (courseId: string, title: string) => void | Promise<void>;
};

type SortableCourseRowProps = {
    course: CourseListItem;
    isReorderMode: boolean;
    pendingCourseId: string | null;
    isReorderPending: boolean;
    onOpenPublishDialog: (
        courseId: string,
        courseTitle: string,
        nextPublished: boolean,
    ) => void;
    onDeleteCourse: (courseId: string, title: string) => void | Promise<void>;
};

const SortableCourseRow = memo(function SortableCourseRow({
    course,
    isReorderMode,
    pendingCourseId,
    isReorderPending,
    onOpenPublishDialog,
    onDeleteCourse,
}: SortableCourseRowProps) {
    const {
        attributes,
        listeners,
        setNodeRef,
        transform,
        transition,
        isDragging,
    } = useSortable({
        id: course.id,
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
            <td className="w-16 px-6 py-4 text-center font-medium text-slate-400">
                <div className="flex items-center justify-center gap-2">
                    {isReorderMode && (
                        <button
                            type="button"
                            className="touch-none cursor-grab text-slate-400 active:cursor-grabbing"
                            aria-label={`Kéo để sắp xếp ${course.title}`}
                            {...attributes}
                            {...listeners}
                        >
                            <GripVertical className="h-4 w-4" />
                        </button>
                    )}
                    <span>#{course.orderIndex}</span>
                </div>
            </td>
            <td className="px-6 py-4 font-medium text-slate-900">
                {course.title}
            </td>
            <td className="hidden px-6 py-4 text-slate-600 sm:table-cell">
                {course.slug}
            </td>
            <td className="px-6 py-4 text-slate-600">HSK {course.hskLevel}</td>
            <td className="hidden px-6 py-4 text-slate-600 md:table-cell">
                {course.hskLevel}
            </td>
            <td className="hidden px-6 py-4 text-slate-600 lg:table-cell">
                {course.totalStudentsEnrolled}
            </td>
            <td className="hidden px-6 py-4 text-slate-600 lg:table-cell">
                {course.totalTopics}
            </td>
            <td className="px-6 py-4">
                <div className="flex items-center gap-2">
                    <Switch
                        checked={course.isPublished}
                        size="default"
                        className="h-6 w-11 border border-slate-300 ring-1 ring-slate-200 data-checked:bg-emerald-600 data-unchecked:bg-slate-300"
                        disabled={pendingCourseId === course.id}
                        onCheckedChange={(checked) =>
                            onOpenPublishDialog(
                                course.id,
                                course.title,
                                checked,
                            )
                        }
                        aria-label={`Chuyển trạng thái xuất bản của ${course.title}`}
                    />
                    <Badge
                        className={`${
                            course.isPublished
                                ? "bg-green-100 text-green-700"
                                : "bg-slate-100 text-slate-600"
                        }`}
                    >
                        {course.isPublished ? "Đã xuất bản" : "Nháp"}
                    </Badge>
                </div>
            </td>
            <td className="px-6 py-4 text-right">
                {pendingCourseId === course.id && (
                    <p className="mb-2 text-[11px] text-slate-500">
                        Đang xử lý...
                    </p>
                )}
                <div className="flex justify-end gap-2">
                    <button
                        type="button"
                        disabled={pendingCourseId === course.id}
                        onClick={() => onDeleteCourse(course.id, course.title)}
                        className="inline-flex items-center gap-1 rounded border border-rose-200 px-2 py-1 text-xs text-rose-700 hover:bg-rose-50 disabled:cursor-not-allowed disabled:opacity-60"
                    >
                        <Trash2 className="h-3.5 w-3.5" />
                        Xóa
                    </button>

                    <Link
                        href={`/cms/lessons/course/${course.id}?tab=settings`}
                    >
                        <button className="rounded border border-slate-200 bg-white px-3 py-1.5 text-xs text-slate-600 shadow-sm transition-colors hover:bg-slate-50 hover:text-amber-600">
                            Chỉnh sửa
                        </button>
                    </Link>

                    <Link href={`/cms/lessons/course/${course.id}`}>
                        <button className="rounded border border-slate-200 bg-white px-3 py-1.5 text-xs text-slate-600 shadow-sm transition-colors hover:bg-slate-50 hover:text-amber-600">
                            Quản lý topic
                        </button>
                    </Link>
                </div>
            </td>
        </tr>
    );
});

export function CourseManagementTable({
    courses,
    isReorderMode,
    pendingCourseId,
    isReorderPending,
    isLoading,
    isError,
    onMoveCourse,
    onOpenPublishDialog,
    onDeleteCourse,
}: CourseManagementTableProps) {
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

        onMoveCourse(String(active.id), String(over.id));
    };

    return (
        <DndContext
            sensors={sensors}
            collisionDetection={closestCenter}
            onDragEnd={handleDragEnd}
        >
            <table className="w-full min-w-full text-left text-sm">
                <thead className="border-b border-slate-200/50 bg-slate-50/50">
                    <tr>
                        <th className="w-16 px-6 py-4 text-center font-semibold text-slate-600">
                            STT
                        </th>
                        <th className="px-6 py-4 font-semibold text-slate-600">
                            Tên khóa học
                        </th>
                        <th className="hidden px-6 py-4 font-semibold text-slate-600 sm:table-cell">
                            Slug
                        </th>
                        <th className="px-6 py-4 font-semibold text-slate-600">
                            Cấp độ
                        </th>
                        <th className="hidden px-6 py-4 font-semibold text-slate-600 md:table-cell">
                            HSK
                        </th>
                        <th className="hidden px-6 py-4 font-semibold text-slate-600 lg:table-cell">
                            Học viên
                        </th>
                        <th className="hidden px-6 py-4 font-semibold text-slate-600 lg:table-cell">
                            Topics
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
                                colSpan={9}
                                className="px-6 py-6 text-center text-slate-500"
                            >
                                Đang tải danh sách khóa học...
                            </td>
                        </tr>
                    )}

                    {!isLoading && isError && (
                        <tr>
                            <td
                                colSpan={9}
                                className="px-6 py-6 text-center text-red-600"
                            >
                                Không thể tải danh sách khóa học.
                            </td>
                        </tr>
                    )}

                    {!isLoading && !isError && courses.length === 0 && (
                        <tr>
                            <td
                                colSpan={9}
                                className="px-6 py-6 text-center text-slate-500"
                            >
                                Không có khóa học phù hợp với bộ lọc hiện tại.
                            </td>
                        </tr>
                    )}

                    <SortableContext
                        items={courses.map((course) => course.id)}
                        strategy={verticalListSortingStrategy}
                    >
                        {!isLoading &&
                            !isError &&
                            courses.map((course) => (
                                <SortableCourseRow
                                    key={course.id}
                                    course={course}
                                    isReorderMode={isReorderMode}
                                    pendingCourseId={pendingCourseId}
                                    isReorderPending={isReorderPending}
                                    onOpenPublishDialog={onOpenPublishDialog}
                                    onDeleteCourse={onDeleteCourse}
                                />
                            ))}
                    </SortableContext>
                </tbody>
            </table>
        </DndContext>
    );
}
