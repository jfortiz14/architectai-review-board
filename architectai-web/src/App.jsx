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

const sampleInput = `A healthcare organization exposes patient integration APIs to external partners using Azure API Management.

Authentication uses OAuth 2.0 Client Credentials and secrets are stored in Azure Key Vault.

Incoming requests are processed through Azure Functions and stored in SQL Server.

Webhook notifications support payload signing but do not currently implement idempotency controls.

The platform includes centralized monitoring and Application Insights logging. Correlation IDs are implemented for API requests but distributed tracing has not been fully adopted.

Retry policies are configured for API failures, however dead-letter queues are not currently implemented.

Environment isolation exists between Development, QA, and Production, but disaster recovery testing has not been formally documented.

Audit logging captures administrative activity and API access events, but PHI access auditing is only partially implemented.

The organization plans to onboard new partners and expand integration capabilities over the next year.`;

const sampleRec = `The platform must support secure partner onboarding using OAuth 2.0 Client Credentials.

The platform must support patient demographic, insurance, and integration event data.

The platform must protect PHI in transit and at rest.

The platform must provide auditability for API access, administrative actions, and PHI access.

The platform must support API versioning and backward - compatible partner integrations.

Webhook notifications must support payload signing and verification.`;

const sampleOpenAPi = `{
    "openapi": "3.0.1",
    "info": {
        "title": "Patient Exchange API",
        "version": "1.0"
    },
    "paths": {
        "/patients": {
            "post": {
                "summary": "Create patient",
                "responses": {
                    "202": {
                        "description": "Accepted"
                    }
                }
            }
        }
    }
}`;

function App() {
    const [projectName, setProjectName] = useState("Enterprise Patient Data Exchange Platform");
    const [description, setDescription] = useState(sampleInput);
    const [openApiSpec, setOpenApiSpec] = useState(sampleOpenAPi);
    const [requirements, setRequirements] = useState(sampleRec);
    const [status, setStatus] = useState("input");
    const [result, setResult] = useState(null);

    const [agentStatuses, setAgentStatuses] = useState({
        "Security Agent": "pending",
        "Integration Agent": "pending",
        "Reliability Agent": "pending",
        "Compliance Agent": "pending",
        "Chief Architect": "pending"
    });

    const runReview = async () => {


        if (!description?.trim()) {
            alert("Architecture Description is required.");
            return;
        }

        setStatus("progress");
        setResult(null);

        const wait = (milliseconds) =>
            new Promise((resolve) => setTimeout(resolve, milliseconds));

        const setProgressState = (currentAgent) => {
            const sequence = [
                "Security Agent",
                "Integration Agent",
                "Reliability Agent",
                "Compliance Agent",
                "Chief Architect (Foundry IQ)"
            ];

            const currentIndex = sequence.indexOf(currentAgent);

            const nextStatuses = {};
            sequence.forEach((agent, agentIndex) => {
                if (agentIndex < currentIndex) {
                    nextStatuses[agent] = "completed";
                } else if (agentIndex === currentIndex) {
                    nextStatuses[agent] = "running";
                } else {
                    nextStatuses[agent] = "pending";
                }
            });

            setAgentStatuses(nextStatuses);
        };

        setProgressState("Security Agent");

        const apiPromise = fetch(API_URL, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({
                projectName,
                architectureDescription: description,
                mermaidDiagram,
                openApiSpec,
                requirements
            })
        }).then(async (response) => {
            if (!response.ok) {
                throw new Error(`API error: ${response.status}`);
            }

            return await response.json();
        });

        try {
            await wait(900);
            setProgressState("Integration Agent");

            await wait(900);
            setProgressState("Reliability Agent");

            await wait(900);
            setProgressState("Compliance Agent");

            await wait(900);
            setProgressState("Chief Architect (Foundry IQ)");

            // Keep Chief Architect running until the real API request completes.
            const data = await apiPromise;

            await wait(600);

            setAgentStatuses({
                "Security Agent": "completed",
                "Integration Agent": "completed",
                "Reliability Agent": "completed",
                "Compliance Agent": "completed",
                "Chief Architect": "completed"
            });

            setResult(data);
            setStatus("result");
        } catch (error) {
            console.error(error);
            alert("API error. Check backend URL or CORS.");
            setStatus("input");
        }
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
                <section className="request-card">
                    <div className="section-header">
                        <div className="eyebrow">Architecture Submission</div>
                        <div className="agent-preview">
                            <span>🛡 Security    </span>
                            <span>⚙ Reliability    </span>
                            <span>🔗 Integration    </span>
                            <span>📜 Compliance    </span>
                            <span>🧠 Chief Architect  <span className="foundry-iq-badge">Foundry IQ</span> </span>
                        </div>
                        <h2>Architecture Review Request</h2>
                        <p>Describe your solution and optionally include diagrams, API contracts, and requirements for deeper agent analysis.</p>
                    </div>

                    <div className="form-group">
                        <label>Project Name</label>
                        <input
                            value={projectName}
                            onChange={(e) => setProjectName(e.target.value)}
                            placeholder="Patient Intake Integration"
                        />
                    </div>

                    <div className="input-split-layout">
                        <div className="primary-input-panel">
                            <div className="form-group">
                                <label>
                                    Architecture Description <span className="required">*</span>
                                </label>
                                <textarea
                                    value={description}
                                    onChange={(e) => setDescription(e.target.value)}
                                    rows={18}
                                    placeholder="Describe the system, data flows, integrations, authentication model, infrastructure, and operational requirements."
                                />
                            </div>

                        </div>

                        <div className="artifact-input-panel">
                            <details className="expandable" open>
                                <summary>Mermaid Diagram <span>Optional</span></summary>
                                <textarea
                                    value={mermaidDiagram}
                                    onChange={(e) => setMermaidDiagram(e.target.value)}
                                    rows={8}
                                    placeholder={"graph TD\n  Partner --> API\n  API --> Function\n  Function --> SQL"}
                                />
                            </details>

                            <details className="expandable" open>
                                <summary>OpenAPI Spec <span>Optional</span></summary>
                                <textarea
                                    value={openApiSpec}
                                    onChange={(e) => setOpenApiSpec(e.target.value)}
                                    rows={8}
                                    placeholder="Paste OpenAPI / Swagger contract here."
                                />
                            </details>

                            <details className="expandable" open>
                                <summary>Requirements <span>Optional</span></summary>
                                <textarea
                                    value={requirements}
                                    onChange={(e) => setRequirements(e.target.value)}
                                    rows={8}
                                    placeholder="Paste business, security, compliance, or scalability requirements here."
                                />
                            </details>
                        </div>
                    </div>



                    <button className="review-button" onClick={runReview}>
                        Run Architecture Review
                    </button>
                </section>
            )}

            {status === "progress" && (
                <ProgressScreen agentStatuses={agentStatuses} />
            )}

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

function ProgressScreen({ agentStatuses }) {
    const agents = [
        ["Security Agent", "Reviewing authentication and authorization patterns", ShieldCheck],
        ["Integration Agent", "Mapping APIs, webhooks, and dependency risks", GitBranch],
        ["Reliability Agent", "Checking retry, queue, and failure handling", Activity],
        ["Compliance Agent", "Reviewing PHI, audit, and access control risks", FileCheck],
        ["Chief Architect (Foundry IQ)", "Powered by Microsoft Foundry IQ Knowledge Base", Brain]
    ];

    const renderStatusIcon = (status) => {
        if (status === "completed") {
            return <CheckCircle2 className="done" size={22} />;
        }

        if (status === "running") {
            return <Loader2 className="spin" size={22} />;
        }

        return <div className="pending-dot" />;
    };

    return (
        <section className="panel progress-panel">
            <h2>Reviewing your architecture...</h2>
            <p>Specialized agents are analyzing the solution from different perspectives.</p>

            <div className="agent-list">
                {agents.map(([name, text, Icon]) => {
                    const status = agentStatuses?.[name] ?? "pending";

                    return (
                        <div className={`agent-row agent-${status}`} key={name}>
                            <Icon size={22} />
                            <div>
                                <strong>{name}</strong>
                                <span>{text}</span>
                            </div>
                            {renderStatusIcon(status)}
                        </div>
                    );
                })}
            </div>
        </section>
    );
}

function AgentTimeline({ agents, knowledgeSources = [] }) {
    const TIMELINE_AGENTS = [
        { key: "Security Agent", icon: ShieldCheck, description: "Authentication, authorization & threat surface" },
        { key: "Reliability Agent", icon: Activity, description: "Retry logic, queues & failure handling" },
        { key: "Integration Agent", icon: GitBranch, description: "APIs, webhooks & external dependencies" },
        { key: "Compliance Agent", icon: FileCheck, description: "PHI, audit trails & access control" },
        { key: "Chief Architect", icon: Brain, description: "Cross-agent consolidation & recommendations", isFoundry: true },
    ];

    const agentLookup = Object.fromEntries(
        (agents ?? []).map(a => [a.agentName, a])
    );

    return (
        <div className="panel timeline-panel">
            <div className="eyebrow">Agent Execution Timeline</div>
            <h3 className="timeline-title">All agents completed</h3>

            <div className="timeline">
                {TIMELINE_AGENTS.map(({ key, icon: Icon, description, isFoundry }, index) => {
                    const apiAgent = agentLookup[key];

                    return (
                        <div className="timeline-item" key={key}>
                            <div className="timeline-connector">
                                <div className={`timeline-icon-wrap ${isFoundry ? "foundry-icon" : ""}`}>
                                    <Icon size={16} />
                                </div>

                                {index < TIMELINE_AGENTS.length - 1 && (
                                    <div className="timeline-line" />
                                )}
                            </div>

                            <div className="timeline-body">
                                <div className="timeline-row">
                                    <strong>
                                        {key}
                                        {isFoundry && (
                                            <span className="foundry-iq-badge">
                                                Foundry IQ
                                            </span>
                                        )}
                                    </strong>

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

                                <p className="timeline-desc">
                                    {isFoundry
                                        ? "Grounding recommendations with enterprise governance standards"
                                        : description}
                                </p>

                                {apiAgent && (
                                    <p className="timeline-findings">
                                        {apiAgent.findingsCount} finding{apiAgent.findingsCount !== 1 ? "s" : ""} identified
                                    </p>
                                )}

                                {isFoundry && knowledgeSources?.length > 0 && (
                                    <div className="foundry-sources">
                                        <div className="foundry-sources-title">
                                            📚 Governance Standards Retrieved
                                        </div>

                                        <div className="foundry-source-list">
                                            {knowledgeSources.map((source) => (
                                                <span className="foundry-source-chip" key={source}>
                                                    {source}
                                                </span>
                                            ))}
                                        </div>
                                    </div>
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
            <div className="section-eyebrow">Chief Architect                                             <span className="foundry-iq-badge">
                Foundry IQ
            </span></div>
            <h2>Remediation Roadmap</h2>

            <div className="roadmap-grid">
                {sections.map((section) => (
                    <div className={`roadmap-card roadmap-${section.theme}`} key={section.title}>
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
                            <span>✓ Chief Architect  <span className="foundry-iq-badge">Foundry IQ</span></span>
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

                <AgentTimeline
                    agents={result.agents}
                    knowledgeSources={result.knowledgeSources}
                />

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

            <ChiefArchitectRoadmap roadmap={result.roadmap} />
            </div>

            <div className="result-actions">
                <button className="export-button" onClick={exportPdf}>
                    Export PDF
                </button>
                <button className="secondary" onClick={onReset}>
                    Run Another Review
                </button>
            </div>
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