import { useState } from 'react';
import axios from 'axios';
import { API_BASE_URL } from '../../util/Constants';
import { useAuth } from '../../features/auth/AuthContext';
import useGetEstacionamentos from '../../Hooks/GetEstacionamentos';
import { showToast } from '../Feedback/ToastNotify';

const CreateAcessoModal = () => {
    const [dropdownOpen, setDropdownOpen] = useState(false);

    const [formData, setFormData] = useState({
        placaVeiculo: '',
        idEstacionamento: '',
    });

    const resetForm = () => {
        setFormData({
            placaVeiculo: '',
            idEstacionamento: '',
        });
        setDropdownOpen(false);
    };

    const { estacionamentos } = useGetEstacionamentos();

    const { token } = useAuth();

    const handleChange = (e) => {
        const { name, value } = e.target;
        setFormData((prev) => ({ ...prev, [name]: value }));
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        try {
            await axios.post(`${API_BASE_URL}/Acesso`, formData, {
                headers: {
                    Authorization: `Bearer ${token}`,
                },
            });
            document.getElementById('create-acesso-modal').close();
            setFormData({ placaVeiculo: '', idEstacionamento: '' });
            showToast({
                message: 'Acesso realizado com sucesso!',
                type: 'success',
                duration: 1000,
                onClose: () => {
                    window.location.reload();
                },
            });
        } catch (error) {
            console.error('Erro ao criar acesso:', error);
            showToast({
                message: 'Erro ao realizar acesso!',
                type: 'error',
                duration: 1000,
                onClose: () => {
                    window.location.reload();
                },
            });
        }
    };

    return (
        <dialog id="create-acesso-modal" className="modal">
            <div className="modal-box bg-background-card-dashboard w-[520px] rounded-[10px] shadow-xl p-8 relative overflow-visible">
                <form method="dialog">
                    <button
                        onClick={resetForm}
                        className="absolute top-4 right-4 text-2xl text-card-dashboard-text"
                    >
                        ✕
                    </button>
                </form>

                <h2 className="text-xl font-bold text-card-dashboard-text text-center mb-6">
                    Realizar novo acesso
                </h2>

                <div className="grid grid-cols-1 gap-4 mb-6">
                    <input
                        required
                        name="placaVeiculo"
                        value={formData.placaVeiculo}
                        onChange={handleChange}
                        type="text"
                        placeholder="Placa do veículo"
                        className="transition duration-300 hover:border-field-border-active hover:ring-1 h-12 w-full pr-4 rounded-lg border border-field-login bg-field-border-login px-4 py-2 text-sm text-texto-login placeholder-placeholder-login focus:outline-none focus:ring-1 focus:ring-field-border-active focus:bg-field-border-login focus:border-field-border-active"
                    />

                    <div className="relative z-50">
                        <button
                            type="button"
                            onClick={() => setDropdownOpen(!dropdownOpen)}
                            className="transition duration-300 h-12 w-full pr-4 rounded-lg border border-field-login bg-field-border-login px-4 py-2 text-sm text-texto-login flex items-center justify-between focus:outline-none focus:ring-1 focus:ring-field-border-active focus:bg-field-border-login focus:border-field-border-active"
                        >
                            <span>
                                {formData.idEstacionamento
                                    ? estacionamentos.find(
                                          (e) =>
                                              e.idEstacionamento ===
                                              formData.idEstacionamento
                                      )?.nome
                                    : 'Selecione um estacionamento'}
                            </span>

                            <svg
                                className={`w-4 h-4 transition-transform ${
                                    dropdownOpen ? 'rotate-180' : ''
                                }`}
                                fill="none"
                                stroke="currentColor"
                                viewBox="0 0 24 24"
                            >
                                <path
                                    strokeLinecap="round"
                                    strokeLinejoin="round"
                                    strokeWidth="2"
                                    d="M19 9l-7 7-7-7"
                                />
                            </svg>
                        </button>

                        {dropdownOpen && (
                            <div className="absolute z-[9999] mt-2 w-full rounded-lg border border-field-login bg-field-border-login shadow-lg overflow-y-auto overflow-x-hidden max-h-60">
                                {estacionamentos.map((est) => (
                                    <div
                                        key={est.idEstacionamento}
                                        onClick={() => {
                                            setFormData((prev) => ({
                                                ...prev,
                                                idEstacionamento:
                                                    est.idEstacionamento,
                                            }));
                                            setDropdownOpen(false);
                                        }}
                                        className="px-4 py-3 text-sm cursor-pointer text-texto-login hover:bg-field-border-active hover:text-white transition"
                                    >
                                        {est.nome}
                                    </div>
                                ))}
                            </div>
                        )}
                    </div>
                </div>

                <button
                    onClick={handleSubmit}
                    className="hover:cursor-pointer w-full h-[50px] font-bold text-white text-[18px] bg-dashboard-create-button rounded-lg hover:opacity-90"
                >
                    CONFIRMAR ACESSO
                </button>
            </div>
        </dialog>
    );
};

export default CreateAcessoModal;
