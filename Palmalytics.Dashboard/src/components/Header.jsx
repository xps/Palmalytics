import '../styles/components/header.css';
import PalmalyticsLogo from '/images/palmalytics.svg';

export default function Header() {
    return (
        <header>
            <h1>
                <img src={PalmalyticsLogo} width={50} height={50} alt="logo" />
                Palmalytics
            </h1>
        </header>
    );
}