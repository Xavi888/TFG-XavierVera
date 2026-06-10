using UnityEngine;

[System.Serializable]
public class PlayerProperties
{
    public int experience;
    public int if_chef_orders;
    public int if_waiter_orders;
    public int for_chef_orders;
    public int for_waiter_orders;
    public int put_chef_orders;
    public int put_waiter_orders;

    public int Level
    {
        get { return experience / 100; }
        set { experience = value * 100; }
    }

    public PlayerProperties()
    {
        experience = 0;
        if_chef_orders = 0;
        if_waiter_orders = 0;
        for_chef_orders = 0;
        for_waiter_orders = 0;
        put_chef_orders = 0;
        put_waiter_orders = 0;
    }

    public void AddExperience(int exp)
    {
        experience += exp;
    }

    public void AddLevel(int level)
    {
        Level += level;
    }

    public void AddIfChefOrders(int ifChefOrders)
    {
        if_chef_orders += ifChefOrders;
    }

    public void AddIfWaiterOrders(int ifWaiterOrders)
    {
        if_waiter_orders += ifWaiterOrders;
    }

    public void AddForChefOrders(int forChefOrders)
    {
        for_chef_orders += forChefOrders;
    }

    public void AddForWaiterOrders(int forWaiterOrders)
    {
        for_waiter_orders += forWaiterOrders;
    }

    public void AddPutChefOrders(int putChefOrders)
    {
        put_chef_orders += putChefOrders;
    }

    public void AddPutWaiterOrders(int putWaiterOrders)
    {
        put_waiter_orders += putWaiterOrders;
    }
}