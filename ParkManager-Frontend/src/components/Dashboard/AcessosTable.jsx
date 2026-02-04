import { useState } from 'react';
import { FaCirclePlus } from 'react-icons/fa6';
import { FaDoorOpen, FaDoorClosed } from 'react-icons/fa';
import { DateConverter } from '../../util/DateConverter';
import { TbClock24 } from 'react-icons/tb';
import ActionButton from '../Form/ActionButton';
import { showToast } from '../Feedback/ToastNotify';
import { IoExitOutline } from 'react-icons/io5';
import FinishAcessoModal from '../Modais/FinishAcessoModal';
import useFinishAcesso from '../../Hooks/FinishAcesso';
import CreateAcessoModal from '../Modais/CreateAcessoModal';

export default function AcessosTable({ data, limit = 3 }) {
    const [currentPage, setCurrentPage] = useState(1);
    const [selectedId, setSelectedId] = useState(null);

    const totalPages = Math.ceil(data.length / limit);
    const paginatedData = data.slice(
        (currentPage - 1) * limit,
        currentPage * limit
    );

    const handleFinishClick = (id) => {
        setSelectedId(id);
        document.getElementById('finish-modal').showModal();
    };

    const { finishEspecificAcesso } = useFinishAcesso();

    return (
        <div className="bg-background-card-dashboard rounded-[20px] shadow-md w-[1589px] h-[400px]">
            <div className="flex flex-row justify-between items-center m-4">
                <p className="text-lg font-bold text-card-dashboard-text">
                    Seus Acessos
                </p>
                <button
                    onClick={() =>
                        document
                            .getElementById('create-acesso-modal')
                            .showModal()
                    }
                    className="flex flex-row justify-between items-center px-4 hover:cursor-pointer rounded-lg text-base text-dashboard-create-button-text bg-dashboard-create-button hover:bg-dashboard-create-button/85 font-bold w-[205px] h-[39px]"
                >
                    <FaCirclePlus className="w-4 h-4 text-dashboard-create-button-text" />
                    <p>REALIZAR ACESSO</p>
                </button>
            </div>

            <div className="overflow-x-auto relative h-[317px]">
                <table className="min-w-full text-left table-fixed">
                    <thead className="bg-card-dashboard-background-table text-card-dashboard-text-table uppercase tracking-wider">
                        <tr>
                            <th className="w-1/8 py-2.5 px-6 text-xs">
                                Estacionamento
                            </th>
                            <th className="w-1/8 py-2.5 px-6 text-xs">Placa</th>
                            <th className="w-2/8 py-2.5 px-6 text-xs">
                                Entrada e Saída
                            </th>
                            <th className="w-1/8 py-2.5 px-6 text-xs">Tipo</th>
                            <th className="w-1/8 py-2.5 px-6 text-xs text-right">
                                Valor
                            </th>
                            <th className="w-1/8 py-2.5 px-6 text-xs text-right"></th>
                        </tr>
                    </thead>
                    <tbody className="text-card-dashboard-text text-[15.5px]">
                        {paginatedData.map((item, index) => (
                            <tr
                                key={index}
                                className="border-t border-card-dashboard-line-table"
                            >
                                <td className="w-2/8 py-3 px-6 truncate opacity-80">
                                    <p className="font-bold">
                                        {item.estacionamento.nome}
                                    </p>
                                </td>
                                <td className="w-1/8 py-3 px-6 truncate opacity-80">
                                    <p className="font-bold">
                                        {item.placaVeiculo}
                                    </p>
                                </td>
                                <td className="w-1/8 py-3 px-6 opacity-80">
                                    {item.tipo === 0 ? (
                                        <>
                                            <div className="flex flex-row gap-x-3 gap-y-1 items-center">
                                                <FaDoorOpen className="w-5 h-5 text-dashboard-tea" />
                                                <p className="text-sm font-bold mt-0.5">
                                                    {DateConverter(
                                                        item.dataHoraEntrada,
                                                        true,
                                                        2
                                                    )}
                                                </p>
                                            </div>
                                            <div className="flex flex-row gap-x-3 gap-y-1 items-center">
                                                <FaDoorClosed className="w-5 h-5 text-dashboard-occupation" />
                                                <p className="text-sm font-bold mt-0.5">
                                                    {item.dataHoraSaida
                                                        ? DateConverter(
                                                              item.dataHoraSaida,
                                                              true,
                                                              2
                                                          )
                                                        : 'Estacionado'}
                                                </p>
                                            </div>
                                        </>
                                    ) : (
                                        <div className="flex flex-row gap-x-3 gap-y-1 items-center">
                                            <TbClock24 className="w-5 h-5 text-dashboard-tea" />
                                            <p className="text-sm font-bold mt-0.5">
                                                24h
                                            </p>
                                        </div>
                                    )}
                                </td>
                                <td className="w-1/8 py-3 px-6 text-base">
                                    {item.dataHoraSaida ? (
                                        <div className="flex flex-row gap-x-3 items-center justify-between bg-dashboard-tea rounded-[6px] w-fit h-[25px] px-3">
                                            <p className="text-dashboard-create-button-text font-bold text-sm">
                                                {item.tipo === 0
                                                    ? 'Por Tempo'
                                                    : item.tipo === 1
                                                      ? 'Diária'
                                                      : item.tipo === 2
                                                        ? 'Mensal'
                                                        : 'Evento'}
                                            </p>
                                        </div>
                                    ) : (
                                        <div className="flex flex-row gap-x-3 items-center justify-between bg-dashboard-occupation rounded-[6px] w-fit h-[25px] px-3">
                                            <p className="text-dashboard-create-button-text font-bold text-sm">
                                                {item.tipo === 0
                                                    ? 'Por Tempo'
                                                    : item.tipo === 1
                                                      ? 'Diária'
                                                      : item.tipo === 2
                                                        ? 'Mensal'
                                                        : 'Evento'}
                                            </p>
                                        </div>
                                    )}
                                </td>
                                <td className="w-1/8 py-3 px-6">
                                    {item.dataHoraSaida ? (
                                        <p className="font-bold text-right">
                                            {`R$ ${item.valorAcesso.toFixed(2).replace('.', ',')}`}
                                        </p>
                                    ) : (
                                        <p className="font-medium text-right">
                                            {`R$ ${item.valorAcesso.toFixed(2).replace('.', ',')}`}
                                        </p>
                                    )}
                                </td>
                                <td className="w-1/8 py-3 px-6">
                                    <div className="flex flex-row gap-x-8 justify-end">
                                        {item.dataHoraSaida ? (
                                            <ActionButton
                                                icon={IoExitOutline}
                                                label="ENCERRAR"
                                                color="text-dashboard-red-500/50 opacity-30 hover:cursor-default"
                                                action={() => {}}
                                            />
                                        ) : (
                                            <ActionButton
                                                icon={IoExitOutline}
                                                label="ENCERRAR"
                                                color="text-dashboard-red-500"
                                                action={() =>
                                                    handleFinishClick(
                                                        item.idAcesso
                                                    )
                                                }
                                            />
                                        )}
                                    </div>
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
                <div className="absolute mt-4 bottom-0 left-0 w-full flex justify-center items-center px-6 space-x-2">
                    <button
                        className={`px-3 font-bold w-[90px] py-1 rounded bg-dashboard-create-button-de ${currentPage === 1 ? 'text-dashboard-create-button/50 cursor-not-allowed' : 'text-dashboard-create-button hover:bg-dashboard-create-button/20'}`}
                        onClick={() =>
                            setCurrentPage((prev) => Math.max(prev - 1, 1))
                        }
                        disabled={currentPage === 1}
                    >
                        Anterior
                    </button>

                    <span className="w-[50px] py-1 rounded text-center font-bold bg-dashboard-create-button text-dashboard-create-button-text">
                        {currentPage}
                    </span>

                    <button
                        className={`px-3 font-bold w-[90px] py-1 rounded bg-dashboard-create-button-de ${currentPage === totalPages ? 'text-dashboard-create-button/50 cursor-not-allowed' : 'text-dashboard-create-button hover:bg-dashboard-create-button/20'}`}
                        onClick={() =>
                            setCurrentPage((prev) =>
                                Math.min(prev + 1, totalPages)
                            )
                        }
                        disabled={currentPage === totalPages}
                    >
                        Próxima
                    </button>
                </div>

                <FinishAcessoModal
                    type={'acesso'}
                    action={() => {
                        handleFinishClick(selectedId);
                        finishEspecificAcesso(selectedId);
                        document.getElementById('finish-modal').close();
                        showToast({
                            message: 'Acesso finalizado com sucesso!',
                            type: 'success',
                            duration: 1000,
                            onClose: () => {
                                window.location.reload();
                            },
                        });
                    }}
                />
                <CreateAcessoModal />
            </div>
        </div>
    );
}
