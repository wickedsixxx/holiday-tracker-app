import React, { useState, useEffect, createContext, useContext } from 'react';
import axios from 'axios';

// --- SABİTLER ---
// API BASE URL'i: Backend adresinizi buraya yazın (Genellikle 5013)
const API_BASE_URL = '/api';

// --- 1. AUTH CONTEXT: Oturum Yönetimi ---
const AuthContext = createContext();

const AuthProvider = ({ children }) => {
    const [user, setUser] = useState(null);
    const [token, setToken] = useState(localStorage.getItem('token'));
    const [loading, setLoading] = useState(true);

    // Hata Çözümü: logout fonksiyonunu useEffect'ten önce tanımladık
    const logout = () => {
        localStorage.removeItem('token');
        setToken(null);
        setUser(null);
        delete axios.defaults.headers.common['Authorization'];
    };

    const login = (token) => {
        localStorage.setItem('token', token);
        setToken(token);
    };

    useEffect(() => {
        if (token) {
            try {
                // Token'ı decode edip kullanıcı bilgisini al
                const payload = JSON.parse(atob(token.split('.')[1]));
                setUser({
                    email: payload.email,
                    role: payload.role,
                    id: payload.nameid
                });
                axios.defaults.headers.common['Authorization'] = `Bearer ${token}`;
            } catch (error) {
                console.error("Token geçersiz:", error);
                logout();
            }
        }
        setLoading(false);
    }, [token]);

    return (
        <AuthContext.Provider value={{ user, token, loading, login, logout }}>
            {children}
        </AuthContext.Provider>
    );
};

const useAuth = () => useContext(AuthContext);

// --- 2. AUTH EKRANLARI (Login / Register) ---

const AuthScreen = () => {
    const [isLogin, setIsLogin] = useState(true);
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [message, setMessage] = useState('');
    const { login } = useAuth();

    const handleSubmit = async (e) => {
        e.preventDefault();
        setMessage('');
        const endpoint = isLogin ? 'auth/login' : 'auth/register';

        try {
            const response = await axios.post(`${API_BASE_URL}/${endpoint}`, { email, password });

            if (response.data.token) {
                login(response.data.token);
            } else {
                setMessage("Kayıt başarılı! Lütfen giriş yapın.");
                if (!isLogin) {
                    setIsLogin(true);
                    setEmail('');
                    setPassword('');
                }
            }
        } catch (error) {
            setMessage(error.response?.data?.message || "Bir hata oluştu.");
        }
    };

    return (
        <div className="min-h-screen flex items-center justify-center bg-gray-50">
            <div className="max-w-md w-full p-8 bg-white shadow-lg rounded-lg">
                <h2 className="text-2xl font-bold text-center mb-6">{isLogin ? 'Giriş Yap' : 'Kayıt Ol'}</h2>
                <form onSubmit={handleSubmit}>
                    <input type="email" value={email} onChange={(e) => setEmail(e.target.value)} placeholder="E-posta" required className="w-full p-3 mb-4 border rounded-md" />
                    <input type="password" value={password} onChange={(e) => setPassword(e.target.value)} placeholder="Şifre" required className="w-full p-3 mb-6 border rounded-md" />
                    <button type="submit" className="w-full bg-indigo-600 text-white p-3 rounded-md hover:bg-indigo-700">
                        {isLogin ? 'Giriş Yap' : 'Kayıt Ol'}
                    </button>
                </form>
                <button onClick={() => setIsLogin(!isLogin)} className="mt-4 text-sm text-indigo-600 hover:underline">
                    {isLogin ? 'Hesabınız yok mu? Kayıt Ol' : 'Zaten bir hesabınız var mı? Giriş Yap'}
                </button>
                {message && <div className={`mt-4 p-3 text-center rounded-md ${message.includes('başarılı') ? 'bg-green-100 text-green-700' : 'bg-red-100 text-red-700'}`}>{message}</div>}
            </div>
        </div>
    );
};

// --- 3. İZİN YÖNETİMİ BİLEŞENLERİ ---

const LeaveRequestForm = ({ onSuccess }) => {
    const { user } = useAuth();
    const [startDate, setStartDate] = useState('');
    const [endDate, setEndDate] = useState('');
    const [message, setMessage] = useState('');

    const handleSubmit = async (e) => {
        e.preventDefault();
        setMessage('');
        try {
            const response = await axios.post(`${API_BASE_URL}/leaverequests`, {
                employeeID: user.id,
                leaveType: 0,
                startDate,
                endDate,
                status: 0
            });
            setMessage(`Talep başarılı! ${response.data.message}`);
            setStartDate('');
            setEndDate('');
            onSuccess();
        } catch (error) {
            setMessage(error.response?.data?.title || error.response?.data?.Message || "Talep oluşturulamadı. (Hafta sonu veya bakiye hatası olabilir)");
        }
    };

    return (
        <div className="p-6 bg-white shadow-md rounded-lg">
            <h3 className="text-xl font-semibold mb-4">Yeni İzin Talep Et</h3>
            <form onSubmit={handleSubmit}>
                <label className="block mb-2 font-medium">Başlangıç Tarihi:</label>
                <input type="date" value={startDate} onChange={(e) => setStartDate(e.target.value)} required className="w-full p-3 mb-4 border rounded-md" />
                <label className="block mb-2 font-medium">Bitiş Tarihi:</label>
                <input type="date" value={endDate} onChange={(e) => setEndDate(e.target.value)} required className="w-full p-3 mb-6 border rounded-md" />
                <button type="submit" className="bg-green-500 text-white p-3 rounded-md hover:bg-green-600">
                    İzin Talep Et
                </button>
            </form>
            {message && <p className={`mt-4 p-2 rounded-md ${message.includes('başarılı') ? 'bg-blue-100 text-blue-700' : 'bg-red-100 text-red-700'}`}>{message}</p>}
        </div>
    );
};

const PendingRequests = () => {
    const [requests, setRequests] = useState([]);
    const [loading, setLoading] = useState(true);
    const [message, setMessage] = useState('');

    const fetchRequests = async () => {
        try {
            const response = await axios.get(`${API_BASE_URL}/leaverequests/pending`);
            setRequests(response.data);
        } catch (error) {
            setMessage("Bekleyen talepler çekilemedi.");
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchRequests();
    }, []);

    const handleAction = async (id, approved) => {
        try {
            const action = approved ? 'approve' : 'reject';
            const response = await axios.post(`${API_BASE_URL}/leaverequests/${id}/${action}`, null, {
                params: { approved }
            });
            setMessage(response.data.message);
            fetchRequests(); // Listeyi yenile
        } catch (error) {
            setMessage(error.response?.data?.message || "İşlem başarısız.");
        }
    };

    if (loading) return <p className="p-6">Yükleniyor...</p>;

    return (
        <div className="p-6">
            <h3 className="text-2xl font-semibold mb-4 border-b pb-2">Onay Bekleyen İzinler ({requests.length})</h3>
            {message && <div className="p-3 bg-yellow-100 text-yellow-800 rounded-md mb-4">{message}</div>}
            {requests.length === 0 ? (
                <p className="text-gray-500">Onay bekleyen izin talebi bulunmamaktadır.</p>
            ) : (
                requests.map(req => (
                    <div key={req.id} className="bg-gray-50 p-4 mb-3 border rounded-lg flex justify-between items-center">
                        <div>
                            <p className="font-bold">{req.employee.firstName} {req.employee.surname} ({req.employee.employeeNo})</p>
                            <p className="text-sm text-gray-600">{new Date(req.startDate).toLocaleDateString()} - {new Date(req.endDate).toLocaleDateString()}</p>
                            {/* Not: API'da net gün sayısını döndürmedik, o yüzden burada gösteremiyoruz. */}
                        </div>
                        <div className="space-x-2">
                            <button onClick={() => handleAction(req.id, true)} className="bg-green-600 text-white py-2 px-4 rounded-md hover:bg-green-700">Onayla</button>
                            <button onClick={() => handleAction(req.id, false)} className="bg-red-600 text-white py-2 px-4 rounded-md hover:bg-red-700">Reddet</button>
                        </div>
                    </div>
                ))
            )}
        </div>
    );
};

// --- 4. ROL BAZLI SAYFALAR (Dashboard) ---

const ManagerDashboard = () => {
    const [activeTab, setActiveTab] = useState('pending');
    return (
        <div className="p-6">
            <h1 className="text-4xl font-extrabold mb-6 text-gray-800">Yönetici Paneli 👑</h1>
            <div className="flex border-b mb-6">
                <button onClick={() => setActiveTab('pending')} className={`py-2 px-4 font-semibold ${activeTab === 'pending' ? 'border-b-4 border-indigo-600 text-indigo-600' : 'text-gray-500 hover:text-indigo-600'}`}>Onay Bekleyen Talepler</button>
                <button onClick={() => setActiveTab('new')} className={`py-2 px-4 font-semibold ${activeTab === 'new' ? 'border-b-4 border-indigo-600 text-indigo-600' : 'text-gray-500 hover:text-indigo-600'}`}>Yeni İzin Talep Et</button>
            </div>
            {activeTab === 'pending' && <PendingRequests />}
            {activeTab === 'new' && <LeaveRequestForm onSuccess={() => setActiveTab('pending')} />}
        </div>
    );
};

const EmployeeDashboard = () => {
    return (
        <div className="p-6">
            <h1 className="text-4xl font-extrabold mb-6 text-gray-800">Çalışan Portalı 👋</h1>
            <LeaveRequestForm onSuccess={() => alert("Talebiniz başarıyla gönderildi!")} />
            <div className="mt-8">
                <h3 className="text-2xl font-semibold mb-4 border-b pb-2">Geçmiş İzin Taleplerim</h3>
                <p className="text-gray-500">
                    Bu listeyi görebilmek için, API'da **`GET /api/leaverequests/myrequests`** endpoint'ini oluşturmamız ve buraya bağlamamız gerekir.
                </p>
            </div>
        </div>
    );
};

// --- 5. ANA UYGULAMA ---

const App = () => {
    const { user, loading, logout } = useAuth();

    if (loading) {
        return <div className="flex justify-center items-center h-screen text-xl text-indigo-600">Yükleniyor...</div>;
    }

    // Uygulama ilk açıldığında kullanıcı yoksa, AuthProvider'dan gelen kullanıcının olmadığını kontrol edip Login ekranını gösterir.
    if (!user) {
        return <AuthScreen />;
    }

    return (
        // Uygulama ana çatısı
        <div className="min-h-screen bg-gray-100">
            <header className="bg-indigo-600 text-white p-4 flex justify-between items-center shadow-lg">
                <span className="text-2xl font-bold">Tatil Takip Sistemi</span>
                <nav className="flex items-center">
                    <span className="mr-4 text-sm">Hoş geldin, **{user.email}** ({user.role})</span>
                    <button onClick={logout} className="bg-red-500 hover:bg-red-600 text-white py-1 px-3 rounded-full text-sm font-medium transition duration-150">
                        Çıkış Yap
                    </button>
                </nav>
            </header>
            <main className="container mx-auto">
                {user.role === 'Manager' && <ManagerDashboard />}
                {user.role === 'Employee' && <EmployeeDashboard />}
                {!['Manager', 'Employee'].includes(user.role) && (
                    <div className="p-6 text-red-600 font-bold">Hata: Tanımsız kullanıcı rolü. Lütfen admin ile iletişime geçin.</div>
                )}
            </main>
        </div>
    );
};

// --- UYGULAMA BAŞLANGICI VE DIŞA AKTARIM ---

// Sadece App bileşenini dışa aktarıyoruz.
// ReactDOM.createRoot ve AuthProvider sarmalaması main.jsx dosyasında yapılmalıdır.
export { AuthProvider, useAuth };
export default App;