import { ViewOrEditExerciseModal } from "@/modules/lesson/components/exercise/view-edit-exercise-modal";
import { Metadata } from "next";

export const metadata: Metadata = {
    title: "Chi tiết bài tập",
    description: "Xem chi tiết bài tập và chỉnh sửa nội dung",
};

export default function ExerciseDetailsPage() {
    return <ViewOrEditExerciseModal />;
}
