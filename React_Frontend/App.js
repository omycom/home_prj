import React, { useState, useEffect } from 'react';
import { Line } from 'react-chartjs-2';
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend,
} from 'chart.js';

ChartJS.register(CategoryScale, LinearScale, PointElement, LineElement, Title, Tooltip, Legend);

function App() {
  const [chartData1, setChartData1] = useState(null);
  const [chartData2, setChartData2] = useState(null);

  // 최근 1시간 데이터 가져오기
  const fetchData = async () => {
    const now = new Date();

    // 한국시간(UTC+9) 적용
    const KST_OFFSET = 9 * 60 * 60 * 1000; // 9시간 (ms)
    const nowKST = new Date(now.getTime() + KST_OFFSET);
    const oneHourAgoKST = new Date(nowKST.getTime() - 1 * 60 * 60 * 1000);

    const start = oneHourAgoKST.toISOString().slice(0, 19);
    const end = nowKST.toISOString().slice(0, 19);

    try {
      const response = await fetch(
        `http://<Your_EndPoint>:5000/api/get_home_prj_data?start=${start}&end=${end}`
      );
      const data = await response.json();

      // 차트용 데이터 정리
      const labels = data.map((d) =>
        new Date(d.insert_datetime).toLocaleTimeString('ko-KR', {
          hour: '2-digit',
          minute: '2-digit',
          timeZone: 'UTC'
        })
      );

      // humidity & volume chart
      setChartData1({
        labels,
        datasets: [
          {
            label: 'Humidity (%)',
            data: data.map((d) => d.humidity),
            borderColor: 'rgba(54, 162, 235, 1)',
            backgroundColor: 'rgba(54, 162, 235, 0.2)',
            yAxisID: 'y',
          },
          {
            label: 'Volume (%)',
            data: data.map((d) => d.volume),
            borderColor: 'rgba(255, 99, 132, 1)',
            backgroundColor: 'rgba(255, 99, 132, 0.2)',
            yAxisID: 'y',
          },
        ],
      });

      // distance chart
      setChartData2({
        labels,
        datasets: [
          {
            label: 'Distance (cm)',
            data: data.map((d) => d.distance),
            borderColor: 'rgba(75, 192, 192, 1)',
            backgroundColor: 'rgba(75, 192, 192, 0.2)',
            yAxisID: 'y',
          },
        ],
      });
    } catch (error) {
      console.error('데이터 가져오기 실패:', error);
    }
  };

  useEffect(() => {
    fetchData();
    const interval = setInterval(fetchData, 60000); // 1분마다 새로고침
    return () => clearInterval(interval);
  }, []);

  return (
    <div style={{ width: '90%', margin: '30px auto' }}>
      <h2>Humidity & Volume (최근 1시간)</h2>
      {chartData1 && (
        <Line
          data={chartData1}
          options={{
            responsive: true,
            plugins: {
              legend: { position: 'top' },
              title: { display: true, text: 'Humidity & Volume (%)' },
            },
            scales: {
              y: { title: { display: true, text: '%' } },
            },
          }}
        />
      )}

      <h2 style={{ marginTop: '50px' }}>Distance (최근 1시간)</h2>
      {chartData2 && (
        <Line
          data={chartData2}
          options={{
            responsive: true,
            plugins: {
              legend: { position: 'top' },
              title: { display: true, text: 'Distance (cm)' },
            },
            scales: {
              y: { title: { display: true, text: 'cm' } },
            },
          }}
        />
      )}
    </div>
  );
}

export default App;
