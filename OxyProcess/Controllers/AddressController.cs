namespace OxyProcess.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.AspNetCore.Mvc;
    using OxyProcess.Data;
    using OxyProcess.Models.UserAddress;

    public class AddressController : Controller
    {
        #region Property
        private readonly ApplicationDbContext _context;
        #endregion

        #region Constructor
        public AddressController(ApplicationDbContext _ApplicationDbContext)
        {
            _context = _ApplicationDbContext;
        }
        #endregion



        #region Action
        /// <summary>
        /// Get Countrylist
        /// </summary>
        /// <returns></returns>
        public List<Country> GetCountryList()
        {

            List<Country> countryList = new List<Country>();
            try
            {
                countryList = _context.Countries.OrderBy(e=>e.CountryName).OrderBy(e=>e.CountryName).ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return countryList;



        }

        /// <summary>
        /// Get Statelist base on country
        /// </summary>
        /// <returns></returns>
        public List<State> GetStateList(int CountryId)
        {

            List<State> StateList = new List<State>();
            try
            {
                StateList = _context.States.Where(p => p.CountryId == CountryId).OrderBy(e=>e.StateName).ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return StateList;

        }

        /// <summary>
        /// Get Citylist base on state
        /// </summary>
        /// <returns></returns>
     
        #endregion


    }
}