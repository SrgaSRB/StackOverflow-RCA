import axios from "axios";
import React, { useEffect, useState } from "react";

export interface ServiceStatusTableDTO {
    CheckDateTime: string; // Velikim slovima
    Status: string; // "OK" or "NOT_OK"
    ServiceName: string;
    ErrorMessage: string;
    ResponseTimeMs: number;
}

export interface ServiceStatusGraphDTO {
    ServiceName: string;
    TotalChecks: number;
    SuccessfulChecks: number;
    FailedChecks: number;
    LastCheckDateTime: string; // ISO string representation of DateTime
}

export interface HealthChecksDTO {
    ServiceStatusTable: ServiceStatusTableDTO[]; // Velikim slovima
    ServiceStatusGraph: ServiceStatusGraphDTO[]; // Velikim slovima
}

const HealthMonitoring: React.FC = () => {
    const [healthData, setHealthData] = useState<HealthChecksDTO>({ 
        ServiceStatusTable: [], 
        ServiceStatusGraph: [] 
    });
    const [autoRefreshInterval, setAutoRefreshInterval] = useState(0);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        const countdown = setInterval(() => {
            setAutoRefreshInterval(prev => {
                if (prev <= 1000) {
                    fetchHealthData();
                    return 30000;
                }
                return prev - 1000;
            });
        }, 1000);

        return () => clearInterval(countdown);
    }, []);


    const fetchHealthData = async () => {
        try {
            setLoading(true);
            setError(null);
            
            const response = await axios.get('http://localhost:64370/');

            if (response.data) {
                setHealthData({
                    ServiceStatusTable: response.data.ServiceStatusTable || [],
                    ServiceStatusGraph: response.data.ServiceStatusGraph || []
                });
            } else {
                console.warn("No data received from API");
                setHealthData({ ServiceStatusTable: [], ServiceStatusGraph: [] });
            }
        } catch (error) {
            console.error("Error fetching health data:", error);
            setError(error instanceof Error ? error.message : 'Unknown error');
            setHealthData({ ServiceStatusTable: [], ServiceStatusGraph: [] });
        } finally {
            setLoading(false);
        }
    };

    if (loading) {
        return (
            <section className="status-dashboard-section">
                <div className="text-block-24">Loading Health Status...</div>
            </section>
        );
    }

    if (error) {
        return (
            <section className="status-dashboard-section">
                <div className="text-block-24">StackOverflow Health Status Dashboard</div>
                <div className="w-layout-blockcontainer container w-container">
                    <div style={{ color: 'red', padding: '20px' }}>
                        Error loading health data: {error}
                        <br />
                        <button onClick={fetchHealthData} style={{ marginTop: '10px' }}>
                            Retry
                        </button>
                    </div>
                </div>
            </section>
        );
    }

    return (
        <section className="status-dashboard-section">
            <div className="text-block-24">StackOverflow Health Status Dashboard</div>
            <div className="w-layout-blockcontainer container w-container">
                <div className="status-dashboard-wrapper">
                    <div className="div-block-15">

                        {healthData?.ServiceStatusGraph && healthData.ServiceStatusGraph.length > 0 ? (
                            healthData.ServiceStatusGraph.map((service, index) => (
                                <div className="div-block-10" key={index}>
                                    <div className="div-block-9">
                                        <div>
                                            <div>{service.ServiceName}</div>
                                        </div>
                                    </div>
                                    <div className="div-block-11">
                                        <div className="div-block-12">
                                            <div className="div-block-13">
                                                <div className="text-block-25">Availability</div>
                                            </div>
                                            <div className="div-block-14">
                                                <div>{service.TotalChecks > 0 ? ((service.SuccessfulChecks / service.TotalChecks) * 100).toFixed(2) : '0.00'}%</div>
                                            </div>
                                        </div>
                                        <div className="div-block-12">
                                            <div className="div-block-13 dark-red">
                                                <div className="text-block-25">Unavailability</div>
                                            </div>
                                            <div className="div-block-14 light-red">
                                                <div>{service.TotalChecks > 0 ? ((service.FailedChecks / service.TotalChecks) * 100).toFixed(2) : '0.00'}%</div>
                                            </div>
                                        </div>
                                        <div className="div-block-12">
                                            <div className="div-block-13 dark-blue">
                                                <div className="text-block-25">Successful</div>
                                            </div>
                                            <div className="div-block-14 light-blue">
                                                <div>{service.SuccessfulChecks}</div>
                                            </div>
                                        </div>
                                        <div className="div-block-12">
                                            <div className="div-block-13 dark-yellow">
                                                <div className="text-block-25">Failed</div>
                                            </div>
                                            <div className="div-block-14 light-yellow">
                                                <div>{service.FailedChecks}</div>
                                            </div>
                                        </div>
                                    </div>
                                    <div className="text-block-26">
                                        In last 3 hours<br />
                                    </div>
                                </div>
                            ))
                        ) : (
                            <div>No service status data available.</div>
                        )}

                    </div>
                    <div className="div-block-16">
                        <div className="div-block-17 header-table">
                            <div>Timestamp</div>
                            <div>Service<br /></div>
                            <div>Status<br /></div>
                            <div>Response Time (ms)<br /></div>
                            <div>Error Message</div>
                        </div>

                        {healthData?.ServiceStatusTable && healthData.ServiceStatusTable.length === 0 && (
                            <div>No health check data available.</div>
                        )}

                        {healthData?.ServiceStatusTable && healthData.ServiceStatusTable.length > 0 && 
                            healthData.ServiceStatusTable.slice(0, 100).map((check, index) => ( 
                                check.Status === "NOT_OK" ? (
                                    <div className="div-block-17 fail-table" key={index}>
                                        <div>{new Date(check.CheckDateTime).toLocaleString()}</div>
                                        <div>{check.ServiceName}</div>
                                        <div id="w-node-_231a8060-07af-5ad8-ba30-bce7ccfffdee-b7b894d8" className="text-block-27 dark-red-text">NOT_OK</div>
                                        <div>{check.ResponseTimeMs}</div>
                                        <div>{check.ErrorMessage || "-"}<br /></div>
                                    </div>
                                ) : (
                                    <div className="div-block-17 success-table" key={index}>
                                        <div>{new Date(check.CheckDateTime).toLocaleString()}</div>
                                        <div>{check.ServiceName}</div>
                                        <div id="w-node-_82578809-da2b-b16f-cce1-af84e6ea6070-b7b894d8" className="text-block-27">OK</div>
                                        <div>{check.ResponseTimeMs}</div>
                                        <div>-<br /></div>
                                    </div>
                                )
                            ))
                        }

                    </div>
                </div>
            </div>
            <div className="div-block-18">
                <div className="text-block-28">Auto-refresh: {autoRefreshInterval / 1000}s</div>
                <button onClick={fetchHealthData} style={{ marginLeft: '10px' }}>
                    Refresh Now
                </button>
            </div>
        </section>
    );
};

export default HealthMonitoring;