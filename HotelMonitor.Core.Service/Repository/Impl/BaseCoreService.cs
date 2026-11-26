using HotelMonitor.Core.DataLayer.Repository;

namespace HotelMonitor.Core.Service;

public class BaseCoreService : IBaseCoreService
{
    protected readonly IHotelMonitorUnitOfWork _unitOfWork;

    public BaseCoreService(IHotelMonitorUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
}
