import SpinnerImage from '/images/spinner.svg';

export default function Spinner() {
    return <div className="spinner">
        <img src={SpinnerImage} alt="Loading..." />
    </div>;
}