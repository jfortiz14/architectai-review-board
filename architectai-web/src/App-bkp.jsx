import mermaid from "mermaid";
import { useEffect, useRef, useState } from "react";
import {
    ShieldCheck,
    GitBranch,
    Activity,
    FileCheck,
    Brain,
    AlertTriangle,
    CheckCircle2,
    //Clock,
    Loader2
} from "lucide-react";
import "./App.css";
import jsPDF from "jspdf";
import html2canvas from "html2canvas";

const API_URL = "https://localhost:7110/api/ArchitectureReview";

const sampleInput = `A healthcare platform exposes a patient intake API to external partners.
The API receives patient demographics, insurance details, and uploaded documents.
Partners authenticate using shared API keys.
The system sends change notifications using webhooks.
Payloads are processed by Azure Functions and stored in SQL Server.
There is no documented retry policy, dead-letter queue, or audit log strategy.`;

function App() {
    const [projectName, setProjectName] = useState("Patient Intake Integration");
    const [description, setDescription] = useState(sampleInput);
    const [status, setStatus] = useState("input");
    const [result, setResult] = useState(null);

    const runReview = async () => {
        setStatus("progress");
        setResult(null);

        setTimeout(async () => {
            try {
                const response = await fetch(API_URL, {
                    method: "POST",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify({
                        projectName,
                        architectureDescription: description,
                        mermaidDiagram,
                        openApiSpec: "",
                        requirements: ""
                    })
                });

                const data = await response.json();
                setResult(data);
                setStatus("result");
            } catch (error) {
                console.error(error);
                alert("API error. Check backend URL or CORS.");
                setStatus("input");
            }
        }, 1800);
    };

    const [mermaidDiagram, setMermaidDiagram] = useState(
        `graph TD
    Partner[External Partner] --> API[Patient Intake API]
    API --> Function[Azure Function]
    Function --> SQL[(SQL Server)]
    Function --> Webhook[Webhook Notifications]`
    );

    return (
        <div className="app-shell">
            <header className="hero">
                <div>
                    <div className="eyebrow">Microsoft Agents League Hackathon </div>
                    <h1>ArchitectAI Review Board </h1>
                    <p className="foundry-badge">
                        Powered by Microsoft Foundry · Enterprise Agent Architecture
                    </p>
                </div>
       
                <div className="hero-badge">
                    <Brain size={26} />
                    Multi-Agent Review
                </div>
            </header>

            {status === "input" && (
                <section className="panel grid-two">
                    <div className="sidebar">
                        <h3>Review Inputs</h3>
                        <div className="nav-item active">Architecture Description</div>
                        <div className="nav-item">Mermaid Diagram</div>
                        <div className="nav-item">OpenAPI Spec</div>
                        <div className="nav-item">Requirements</div>
                    </div>

                    <div className="input-area">
                        <label>Project Name</label>
                        <input
                            value={projectName}
                            onChange={(e) => setProjectName(e.target.value)}
                        />

                        <label>Architecture Description</label>
                        <textarea
                            value={description}
                            onChange={(e) => setDescription(e.target.value)}
                        />

                        <label>Mermaid Diagram</label>
                        <textarea
                            value={mermaidDiagram}
                            onChange={(e) => setMermaidDiagram(e.target.value)}
                        />
                        <button onClick={runReview}>Run Architecture Review</button>
                    </div>
                </section>
            )}

            {status === "progress" && <ProgressScreen />}

            {status === "result" && result && (
               
                <ResultsDashboard
  result={result}
  mermaidDiagram={mermaidDiagram}
  onReset={() => setStatus("input")}
/>
            )}
        </div>
    );
}

function ProgressScreen() {
    const agents = [
        ["Security Agent", "Reviewing authentication and authorization patterns", ShieldCheck],
        ["Integration Agent", "Mapping APIs, webhooks, and dependency risks", GitBranch],
        ["Reliability Agent", "Checking retry, queue, and failure handling", Activity],
        ["Compliance Agent", "Reviewing PHI, audit, and access control risks", FileCheck],
        ["Chief Architect", "Consolidating agent findings", Brain]
    ];

    return (
        <section className="panel progress-panel">
            <h2>Reviewing your architecture...</h2>
            <p>Specialized agents are analyzing the solution from different perspectives.</p>

            <div className="agent-list">
                {agents.map(([name, text, Icon], index) => (
                    <div className="agent-row" key={name}>
                        <Icon size={22} />
                        <div>
                            <strong>{name}</strong>
                            <span>{text}</span>
                        </div>
                        {index < 3 ? (
                            <CheckCircle2 className="done" size={22} />
                        ) : (
                            <Loader2 className="spin" size={22} />
                        )}
                    </div>
                ))}
            </div>
        </section>
    );
}

function AgentTimeline({ agents }) {
    const TIMELINE_AGENTS = [
        { key: "Security Agent",     icon: ShieldCheck, description: "Authentication, authorization & threat surface" },
        { key: "Reliability Agent",  icon: Activity,    description: "Retry logic, queues & failure handling" },
        { key: "Integration Agent",  icon: GitBranch,   description: "APIs, webhooks & external dependencies" },
        { key: "Compliance Agent",   icon: FileCheck,   description: "PHI, audit trails & access control" },
        { key: "Chief Architect (Foundry IQ)",    icon: Brain,       description: "Cross-agent consolidation & recommendations" },
    ];

    // Build a lookup from the API response so we can show per-agent risk score
    const agentLookup = Object.fromEntries(
        (agents ?? []).map(a => [a.agentName, a])
    );

    return (
        <div className="panel timeline-panel">
            <div className="eyebrow">Agent Execution Timeline</div>
            <h3 className="timeline-title">All agents completed</h3>
            <div className="timeline">
                {TIMELINE_AGENTS.map(({ key, icon: Icon, description }, index) => {
                    const apiAgent = agentLookup[key];
                    return (
                        <div className="timeline-item" key={key}>
                            <div className="timeline-connector">
                                <div className="timeline-icon-wrap">
                                    <Icon size={16} />
                                </div>
                                {index < TIMELINE_AGENTS.length - 1 && (
                                    <div className="timeline-line" />
                                )}
                            </div>
                            <div className="timeline-body">
                                <div className="timeline-row">
                                    <strong>{key}</strong>
                                    <span className="timeline-status-badge">
                                        <CheckCircle2 size={13} />
                                        Completed
                                    </span>
                                    {apiAgent && (
                                        <span className="timeline-score">
                                            Risk score: {apiAgent.riskScore}/100
                                        </span>
                                    )}
                                </div>
                                <p className="timeline-desc">{description}</p>
                                {apiAgent && (
                                    <p className="timeline-findings">
                                        {apiAgent.findingsCount} finding{apiAgent.findingsCount !== 1 ? "s" : ""} identified
                                    </p>
                                )}
                            </div>
                        </div>
                    );
                })}
            </div>
        </div>
    );
}

function HealthScoreCard({ overallScore }) {
    const health = Math.max(0, Math.min(100, 100 - (overallScore ?? 0)));

    const colorClass =
        health >= 75 ? "health-good" :
        health >= 50 ? "health-medium" :
        health >= 25 ? "health-poor" :
                       "health-critical";

    const label =
        health >= 75 ? "Healthy" :
        health >= 50 ? "Moderate" :
        health >= 25 ? "At Risk" :
                       "Critical";

    return (
        <div className={`health-card ${colorClass}`}>
            <div className="health-header">
                <div>
                    <div className="eyebrow">Architecture Health Score</div>
                    <div className="health-number">{health}<span>%</span></div>
                </div>
                <div className={`health-label-badge ${colorClass}`}>{label}</div>
            </div>
            <div className="health-bar-track">
                <div
                    className={`health-bar-fill ${colorClass}`}
                    style={{ width: `${health}%` }}
                />
            </div>
            <div className="health-legend">
                <span>0 — Critical</span>
                <span>50 — Moderate</span>
                <span>100 — Healthy</span>
            </div>
        </div>
    );
}

function ChiefArchitectRoadmap({ roadmap }) {
    if (!roadmap) return null;

    const sections = [
        {
            title: "Quick Wins",
            subtitle: "1-2 weeks",
            icon: "⚡",
            items: roadmap.quickWins || [],
            theme: "green"
        },
        {
            title: "Strategic Improvements",
            subtitle: "1-3 months",
            icon: "🏗️",
            items: roadmap.strategicImprovements || [],
            theme: "orange"
        },
        {
            title: "Long-Term Vision",
            subtitle: "3-12 months",
            icon: "🚀",
            items: roadmap.longTermVision || [],
            theme: "blue"
        }
    ];

    return (
        <section className="roadmap-section">
            <div className="section-eyebrow">Chief Architect Agent</div>
            <h2>Remediation Roadmap</h2>

            <div className="roadmap-grid">
                {sections.map((section) => (
                    <div className={`roadmap-card roadmap-${section.theme}`}>
                        <div className="roadmap-card-header">
                            <div className="roadmap-icon">{section.icon}</div>
                            <div>
                                <h3>{section.title}</h3>
                                <span>{section.subtitle}</span>
                            </div>
                        </div>

                        <ul>
                            {section.items.map((item, index) => (
                                <li key={index}>{item}</li>
                            ))}
                        </ul>
                    </div>
                ))}
            </div>
        </section>
    );
}


function ResultsDashboard({ result, mermaidDiagram, onReset }) {

    const exportPdf = async () => {
        const element = document.getElementById("architecture-review-report");

        if (!element) return;

        const canvas = await html2canvas(element, {
            scale: 2,
            backgroundColor: "#0f172a"
        });

        const imgData = canvas.toDataURL("image/png");

        const pdf = new jsPDF("p", "mm", "a4");
        const pageWidth = pdf.internal.pageSize.getWidth();
        const pageHeight = pdf.internal.pageSize.getHeight();

        const imgWidth = pageWidth;
        const imgHeight = (canvas.height * imgWidth) / canvas.width;

        let heightLeft = imgHeight;
        let position = 0;

        pdf.addImage(imgData, "PNG", 0, position, imgWidth, imgHeight);
        heightLeft -= pageHeight;

        while (heightLeft > 0) {
            position = heightLeft - imgHeight;
            pdf.addPage();
            pdf.addImage(imgData, "PNG", 0, position, imgWidth, imgHeight);
            heightLeft -= pageHeight;
        }

        pdf.save(`${result.projectName || "Architecture-Review"}.pdf`);
    };

    return (
  
        <section className="results">
            <div id="architecture-review-report">
            <div className="exec-summary">
                <div className="exec-summary-top">
                    <div className="eyebrow">Executive Summary</div>
                    <h2>Architecture Review Results</h2>
                    <p className="exec-project">{result.projectName}</p>
                </div>
                <div className="executive-summary-agents">
                    <span>Analyzed by 5 specialized AI agents</span>

                    <div className="agent-badges">
                        <span>✓ Security</span>
                        <span>✓ Reliability</span>
                        <span>✓ Integration</span>
                        <span>✓ Compliance</span>
                        <span>✓ Chief Architect</span>
                    </div>
                </div>
                <div className="exec-metrics">
                    <div className="exec-metric">
                        <span>Overall Risk</span>
                        <strong className={`risk-badge risk-${(result.overallRisk ?? "").toLowerCase()}`}>
                            {result.overallRisk}
                        </strong>
                    </div>
                    <div className="exec-metric">
                        <span>Risk Score</span>
                        <strong className="exec-score">
                            {result.overallScore}<small>/100</small>
                        </strong>
                    </div>
      
                </div>
                <div className="exec-summary-body">
                    <Brain size={20} />
                    <p>{result.summary}</p>
                </div>
            </div>

            <HealthScoreCard overallScore={result.overallScore} />

            <MermaidDiagramPreview diagram={mermaidDiagram} />

            <AgentTimeline agents={result.agents} />
            <script>console.log(result.agents);</script>

            <div className="cards">
                {result.agents.map((agent) => (
                    <div className="agent-card" key={agent.agentName}>
                        <h3>{agent.agentName}</h3>
                        <p>{agent.findingsCount} findings</p>
                        <strong>{agent.riskScore}/100</strong>
                    </div>
                ))}
            </div>

            <div className="content-grid">
                <div className="panel">
                    <h3>Top Risks</h3>
                    {result.topRisks.map((risk, i) => (
                        <div className="risk-row" key={i}>
                            <AlertTriangle size={20} />
                            <div>
                                <strong>{risk.title}</strong>
                                <span>{risk.severity} · {risk.ownerAgent}</span>
                            </div>
                        </div>
                    ))}
                </div>

                <div className="panel">
                    <h3>Recommended Actions</h3>
                    {result.recommendedActions.map((action) => (
                        <div className="action-row" key={action.priority}>
                            <div className="priority">{action.priority}</div>
                            <div>
                                <strong>{action.action}</strong>
                                <span>{action.impact}</span>
                            </div>
                        </div>
                    ))}
                </div>
            </div>

            <div className="panel">
                <h3>Agent Findings</h3>
                {result.agents.map((agent) => (
                    <div className="finding-group" key={agent.agentName}>
                        <h4>{agent.agentName}</h4>
                        {agent.findings.map((finding, i) => (
                            <div className="finding" key={i}>
                                <div>
                                    <strong>{finding.title}</strong>
                                    <span>{finding.severity} · {finding.category}</span>
                                </div>
                                <p>{finding.description}</p>
                                <em>{finding.recommendation}</em>
                            </div>
                        ))}
                    </div>
                ))}
                </div>
                    </div>
            <ChiefArchitectRoadmap roadmap={result.roadmap} />
                <button className="export-button" onClick={exportPdf}>
                    Export PDF
                </button>
            <button className="secondary" onClick={onReset}>
                Run Another Review
            </button>
            </section>

    );
}

function MermaidDiagramPreview({ diagram }) {
    const ref = useRef(null);

    useEffect(() => {
        if (!diagram || !ref.current) return;

        mermaid.initialize({
            startOnLoad: false,
            theme: "dark",
            securityLevel: "loose"
        });

        const renderDiagram = async () => {
            try {
                const id = `mermaid-${Date.now()}`;
                const { svg } = await mermaid.render(id, diagram);
                ref.current.innerHTML = svg;
            } catch (error) {
                ref.current.innerHTML = `
          <div class="mermaid-error">
            Unable to render Mermaid diagram.`+ error + `Please check the syntax.
          </div>
        `;
            }
        };

        renderDiagram();
    }, [diagram]);

    if (!diagram || !diagram.trim()) return null;

    return (
        <section className="diagram-preview-section">
            <div className="section-eyebrow">Architecture Visualization</div>
            <h2>Mermaid Diagram Preview</h2>
            <div className="diagram-preview" ref={ref}></div>
        </section>
    );
}

export default App;