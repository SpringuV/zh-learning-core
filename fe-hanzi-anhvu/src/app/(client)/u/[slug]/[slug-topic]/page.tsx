import ExerciseClientComponent from "@/modules/lesson/components/exercise/client.exercise";
import { Metadata } from "next";

export const metadata: Metadata = {
    title: "Luyện tập - HanziAnhVu",
};

const TopicExercisePage = () => {
    return <ExerciseClientComponent />;
};

export default TopicExercisePage;
