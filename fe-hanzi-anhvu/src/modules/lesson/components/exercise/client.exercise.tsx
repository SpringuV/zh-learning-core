import { useParams } from "next/navigation";

const ExerciseClientComponent = () => {
    const { "slug-topic": slugTopic } = useParams();
    console.log("Slug topic:", slugTopic);
    return <div>Exercise Client Component</div>;
};

export default ExerciseClientComponent;
