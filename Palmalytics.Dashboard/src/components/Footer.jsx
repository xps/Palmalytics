import '../styles/components/footer.css';
import { useState } from 'react';
import GitHubLogo from '/images/github.svg';
import NuGetLogo from '/images/nuget.svg';
import * as api from '../helpers/api';

export default function Footer() {
    const [version, setVersion] = useState(null);

    api.getVersion().then(data => {
        setVersion(data.version);
    });

    return (
        <footer>
            <div className="container">
                <hr />
                { version &&
                    <div className="version">
                        Palmalytics {version}
                    </div>
                }
                <div className="links">
                    <div>
                        <a href="https://github.com/xps/Palmalytics" title="See the project on GitHub">
                            <img src={GitHubLogo} alt="GitHub Logo" height={40} />
                        </a>
                    </div>
                    {/* <div>
                        <a href="https://www.nuget.org/" title="See the project on NuGet">
                            <img src={NuGetLogo} alt="NuGet Logo" height={40} />
                        </a>
                    </div> */}
                </div>
            </div>
        </footer>
    );
}